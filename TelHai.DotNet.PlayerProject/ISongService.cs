using System.Collections.Generic;

namespace TelHai.DotNet.PlayerProject
{
    public interface ISongService
    {
        List<MusicTrack> GenerateSongs(int count);
    }
}
