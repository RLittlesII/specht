using System.Collections.Generic;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Cli.Help;
using Spectre.Console.Rendering;

namespace Specht.Tool;

/// <summary>Renders the wordmark in the root help header.</summary>
/// <implements epic="0001" feature="F2" claim="B-048" />
internal sealed class SpechtHelpProvider : HelpProvider
{
    /// <summary>Creates the provider using the app's help settings.</summary>
    /// <implements epic="0001" feature="F2" claim="B-048" />
    public SpechtHelpProvider(ICommandAppSettings settings) : base(settings)
    {
    }

    /// <inheritdoc />
    /// <implements epic="0001" feature="F2" claim="B-048" />
    public override IEnumerable<IRenderable> GetHeader(ICommandModel model, ICommandInfo? command)
    {
        if (command is null or { Parent: null, IsDefaultCommand: true })
        {
            yield return new Text(
                """
                 ####  ####   #####   ####  #   #  #####
                #      #   #  #      #      #   #    #
                 ###   ####   ####   #      #####    #
                    #  #      #      #      #   #    #
                ####   #      #####   ####  #   #    #


                """,
                new Style(Color.Cyan)
            );
        }
    }
}
