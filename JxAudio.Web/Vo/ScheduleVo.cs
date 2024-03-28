using Longbow.Tasks;

namespace JxAudio.Web.Vo;

public class ScheduleVo
{
    public string? Name { get; set; }

    public SchedulerStatus Status { get; set; }

    public DateTimeOffset? NextRuntime { get; set; }

    public DateTimeOffset? LastRuntime { get; set; }

    public TriggerResult LastRunResult{ get; set; }

    public IScheduler? Scheduler { get; set; }

    public DateTimeOffset CreatedTime { get; set; }
}