using Microsoft.AspNetCore.Components;

namespace JxAudio.Web.Components.Components.Install;

public class StepBase: ComponentBase
{
    [Parameter]
    public Action? Prev { get; set; }

    [Parameter]
    public Action? Next { get; set; }
}