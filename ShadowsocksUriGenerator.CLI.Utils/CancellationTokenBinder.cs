using System.CommandLine.Binding;
using System.Threading;

namespace ShadowsocksUriGenerator.CLI.Utils;

public class CancellationTokenBinder : BinderBase<CancellationToken>
{
    protected override CancellationToken GetBoundValue(BindingContext bindingContext) =>
        (CancellationToken)bindingContext.GetService(typeof(CancellationToken))!;
}
