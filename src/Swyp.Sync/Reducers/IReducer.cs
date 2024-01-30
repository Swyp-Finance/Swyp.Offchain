using PallasDotnet.Models;

namespace Swyp.Sync.Reducers;

public interface IReducer
{
    Task RollForwardAsync(NextResponse response);
    Task RollBackwardAsync(NextResponse response);
}