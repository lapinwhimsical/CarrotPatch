using System;
using System.Numerics;
using CarrotPatch.Features.RabbitEars;
using Dalamud.Bindings.ImGui;

namespace CarrotPatch.UI;

public sealed class RecentSignalsWindow
{
    private readonly RabbitEarsService rabbitEarsService;

    public RecentSignalsWindow(RabbitEarsService rabbitEarsService)
    {
        this.rabbitEarsService = rabbitEarsService;
    }

    public bool IsOpen { get; set; }

    public void Draw()
    {
        if (!this.IsOpen)
            return;

        var isOpen = this.IsOpen;
        ImGui.SetNextWindowSize(new Vector2(520f, 260f), ImGuiCond.FirstUseEver);

        if (!ImGui.Begin("CarrotPatch##recent-signals", ref isOpen))
        {
            this.IsOpen = isOpen;
            ImGui.End();
            return;
        }

        this.IsOpen = isOpen;

        if (ImGui.Button("Clear"))
        {
            this.rabbitEarsService.ClearRecentSignals();
        }

        ImGui.Separator();

        if (ImGui.BeginTable("RecentSignalsTable", 5, ImGuiTableFlags.BordersInnerH | ImGuiTableFlags.RowBg | ImGuiTableFlags.Resizable))
        {
            ImGui.TableSetupColumn("When");
            ImGui.TableSetupColumn("Type");
            ImGui.TableSetupColumn("Player");
            ImGui.TableSetupColumn("Distance");
            ImGui.TableSetupColumn("Visible");
            ImGui.TableHeadersRow();

            foreach (var signal in this.rabbitEarsService.RecentSignals)
            {
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(FormatAge(signal.SeenAt));

                ImGui.TableNextColumn();
                ImGui.TextUnformatted(signal.Type == RabbitEarsSignalType.Tell ? "Tell" : "Targeting");

                ImGui.TableNextColumn();
                ImGui.TextUnformatted(string.IsNullOrWhiteSpace(signal.SenderWorld)
                    ? signal.SenderName
                    : $"{signal.SenderName} @ {signal.SenderWorld}");

                ImGui.TableNextColumn();
                ImGui.TextUnformatted(signal.Distance.HasValue ? $"{MathF.Round(signal.Distance.Value)}y" : "-");

                ImGui.TableNextColumn();
                ImGui.TextUnformatted(signal.IsVisible ? "Yes" : "No");
            }

            ImGui.EndTable();
        }

        ImGui.End();
    }

    private static string FormatAge(DateTime seenAt)
    {
        var age = DateTime.UtcNow - seenAt;
        if (age.TotalSeconds < 60)
            return $"{Math.Max(0, (int)age.TotalSeconds)}s";

        if (age.TotalMinutes < 60)
            return $"{(int)age.TotalMinutes}m";

        return $"{(int)age.TotalHours}h";
    }
}
