using Godot;
using System.Collections.Generic;
using System.Linq;

namespace MaxGodotFrame;

/// <summary>Host-owned playback. Does not create buses, persist preferences, or choose streams.</summary>
[GlobalClass]
public partial class FrameAudio : Node
{
    [Export] public StringName MusicBus { get; set; } = "Master";
    [Export] public StringName EffectsBus { get; set; } = "Master";
    [Export(PropertyHint.Range, "1,32,1")] public int MaxVoices { get; set; } = 8;
    private AudioStreamPlayer? music;
    private readonly List<AudioStreamPlayer> voices = new();
    public override void _Ready() => ProcessMode = ProcessModeEnum.Always;
    public AudioStreamPlayer PlayMusic(AudioStream stream)
    {
        if (music == null)
        {
            music = new AudioStreamPlayer { Name = "Music", ProcessMode = ProcessModeEnum.Always };
            AddChild(music);
        }
        music.Bus = MusicBus;
        if (music.Stream != stream || !music.Playing) { music.Stream = stream; music.Play(); }
        return music;
    }
    public AudioStreamPlayer PlayEffect(AudioStream stream, bool pauseWithTree = true)
    {
        while (voices.Count >= System.Math.Clamp(MaxVoices, 1, 32)) RemoveVoice(voices[0]);
        var player = new AudioStreamPlayer { Stream = stream, Bus = EffectsBus,
            ProcessMode = pauseWithTree ? ProcessModeEnum.Pausable : ProcessModeEnum.Always };
        voices.Add(player); AddChild(player);
        player.Finished += () => RemoveVoice(player);
        player.Play(); return player;
    }
    public int GetVoiceCount() => voices.Count;
    public void PauseMusic(bool paused) { if (music != null) music.StreamPaused = paused; }
    public void StopMusic() { if (music != null) { music.Stop(); music.Stream = null; } }
    public void StopEffects() { foreach (var voice in voices.ToArray()) RemoveVoice(voice); }
    public void StopAll() { StopMusic(); StopEffects(); }
    public override void _ExitTree()
    {
        music?.Stop();
        if (music != null) music.Stream = null;
        foreach (var voice in voices) { voice.Stop(); voice.Stream = null; }
        voices.Clear();
    }
    private void RemoveVoice(AudioStreamPlayer player)
    {
        if (!voices.Remove(player)) return;
        player.Stop(); player.QueueFree();
    }
}
