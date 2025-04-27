using ShadowsocksUriGenerator.Data;
using ShadowsocksUriGenerator.OnlineConfig;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ShadowsocksUriGenerator.CLI;

public static class ServiceCommand
{
    public static async Task<int> Run(CancellationToken cancellationToken = default)
    {
        while (true)
        {
            var (users, loadUsersErrMsg) = await Users.LoadUsersAsync(cancellationToken);
            if (loadUsersErrMsg is not null)
            {
                Console.WriteLine(loadUsersErrMsg);
                return 1;
            }

            var (loadedNodes, loadNodesErrMsg) = await Nodes.LoadNodesAsync(cancellationToken);
            if (loadNodesErrMsg is not null)
            {
                Console.WriteLine(loadNodesErrMsg);
                return 1;
            }
            using var nodes = loadedNodes;

            var (settings, loadSettingsErrMsg) = await Settings.LoadSettingsAsync(cancellationToken);
            if (loadSettingsErrMsg is not null)
            {
                Console.WriteLine(loadSettingsErrMsg);
                return 1;
            }

            if (settings.ServicePullFromServers)
            {
                try
                {
                    await nodes.PullGroupsAsync(ReadOnlyMemory<string>.Empty, users, settings, cancellationToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

                Console.WriteLine("Pulled from servers.");
            }
            if (settings.ServiceDeployToServers)
            {
                var errMsgs = nodes.DeployAllOutlineServers(users, cancellationToken);

                await foreach (var errMsg in errMsgs)
                {
                    Console.WriteLine(errMsg);
                }

                Console.WriteLine("Deployed to servers.");
            }
            if (settings.ServiceGenerateOnlineConfig)
            {
                var errMsg = await SIP008StaticGen.GenerateAndSave(users, nodes, settings, cancellationToken);
                if (errMsg is not null)
                    Console.Write(errMsg);

                Console.WriteLine("Generated online config.");
            }
            if (settings.ServiceRegenerateOnlineConfig)
            {
                SIP008StaticGen.Remove(users, settings);

                Console.WriteLine("Cleaned online config.");

                var errMsg = await SIP008StaticGen.GenerateAndSave(users, nodes, settings, cancellationToken);
                if (errMsg is not null)
                    Console.Write(errMsg);

                Console.WriteLine("Generated online config.");
            }

            var saveUsersErrMsg = await Users.SaveUsersAsync(users, cancellationToken);
            if (saveUsersErrMsg is not null)
            {
                Console.WriteLine(saveUsersErrMsg);
                return 1;
            }

            var saveNodesErrMsg = await Nodes.SaveNodesAsync(nodes, cancellationToken);
            if (saveNodesErrMsg is not null)
            {
                Console.WriteLine(saveNodesErrMsg);
                return 1;
            }

            try
            {
                await Task.Delay(settings.ServiceRunIntervalSecs * 1000, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                return 0;
            }
        }
    }
}
