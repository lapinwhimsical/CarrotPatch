using System;
using System.Collections.Generic;
using System.Numerics;
using CarrotPatch.Features.RabbitEars;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Bindings.ImGui;
using Dalamud.Plugin.Services;

namespace CarrotPatch.UI;

public sealed class RecentSignalsWindow
{
    private static readonly Vector4 ActiveSignalColor = new(1f, 0.15f, 0.1f, 1f);

    private readonly RabbitEarsService rabbitEarsService;
    private readonly ITargetManager targetManager;
    private readonly Configuration configuration;

    public RecentSignalsWindow(RabbitEarsService rabbitEarsService, ITargetManager targetManager, Configuration configuration)
    {
        this.rabbitEarsService = rabbitEarsService;
        this.targetManager = targetManager;
        this.configuration = configuration;
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

        if (ImGui.BeginTable("RecentSignalsTable", 4, ImGuiTableFlags.BordersInnerH | ImGuiTableFlags.RowBg | ImGuiTableFlags.Resizable))
        {
            ImGui.TableSetupColumn("When");
            ImGui.TableSetupColumn("Type");
            ImGui.TableSetupColumn("Player");
            ImGui.TableSetupColumn("Visible");
            ImGui.TableHeadersRow();

            var latestSignalKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var signal in this.rabbitEarsService.RecentSignals)
            {
                var isLatestForPlayer = latestSignalKeys.Add(GetSignalKey(signal));
                if (this.configuration.ShowOnlyLatestSignalPerPlayer && !isLatestForPlayer)
                    continue;

                var gameObject = this.rabbitEarsService.ResolveSignalObject(signal);
                var isVisible = gameObject is not null;
                var isActiveLatestSignal = isLatestForPlayer && this.rabbitEarsService.IsSignalActive(signal);

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                DrawText(FormatAge(signal.SeenAt), isActiveLatestSignal);

                ImGui.TableNextColumn();
                DrawText(signal.Type == RabbitEarsSignalType.Tell ? "Tell" : "Targeting", isActiveLatestSignal);

                ImGui.TableNextColumn();
                this.DrawPlayerCell(signal, gameObject, isActiveLatestSignal);

                ImGui.TableNextColumn();
                DrawText(isVisible ? "Yes" : "No", isActiveLatestSignal);
            }

            ImGui.EndTable();
        }

        ImGui.End();
    }

    private void DrawPlayerCell(RecentSignal signal, IGameObject? gameObject, bool isActiveSignal)
    {
        var label = string.IsNullOrWhiteSpace(signal.SenderWorld)
            ? signal.SenderName
            : $"{signal.SenderName} @ {signal.SenderWorld}";

        if (gameObject is null)
        {
            DrawText(label, isActiveSignal);
            return;
        }

        if (isActiveSignal)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, ActiveSignalColor);
        }

        ImGui.Selectable(label, false, ImGuiSelectableFlags.None, Vector2.Zero);
        if (isActiveSignal)
        {
            ImGui.PopStyleColor();
        }

        if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
        {
            this.TargetPlayer(gameObject);
        }

        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
        {
            this.rabbitEarsService.ShowManualMarker(signal);
        }
    }

    private void TargetPlayer(IGameObject gameObject)
    {
        this.targetManager.Target = gameObject;
    }

    private static void DrawText(string text, bool isActiveSignal)
    {
        if (!isActiveSignal)
        {
            ImGui.TextUnformatted(text);
            return;
        }

        ImGui.TextColored(ActiveSignalColor, text);
    }

    private static string GetSignalKey(RecentSignal signal)
        => signal.GameObjectId.HasValue
            ? $"id:{signal.GameObjectId.Value}"
            : $"name:{TellParserCore.NormalizeName(signal.SenderName)}@{signal.SenderWorld ?? string.Empty}";

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
