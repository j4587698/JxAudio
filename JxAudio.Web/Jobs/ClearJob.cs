using FreeSql;
using JxAudio.Core.Entity;
using Longbow.Tasks;
using Serilog;

namespace JxAudio.Web.Jobs;

public class ClearJob: ITask
{
    public async Task Execute(IServiceProvider provider, CancellationToken cancellationToken)
    {
        Log.Information("ClearJob is running.");

        try
        {
            var albumRepo = BaseEntity.Orm.GetRepository<AlbumEntity>();
            var albums = await albumRepo.DeleteCascadeByDatabaseAsync(x => x.TrackEntities!.Count == 0, cancellationToken);
            
            Log.Information($"ClearJob delete {albums?.Count ?? 0} empty albums.");
            
            var artistRepo = BaseEntity.Orm.GetRepository<ArtistEntity>();
            var artists = await artistRepo.DeleteCascadeByDatabaseAsync(x => x.TrackEntities!.Count == 0, cancellationToken);
            
            Log.Information($"ClearJob delete {artists?.Count ?? 0} empty artists.");
            
            var genreRepo = BaseEntity.Orm.GetRepository<GenreEntity>();
            var genres = await genreRepo.DeleteCascadeByDatabaseAsync(x => x.TrackEntities!.Count == 0, cancellationToken);
            
            Log.Information($"ClearJob delete {genres?.Count ?? 0} empty artists.");
        }
        catch (Exception e)
        {
            Log.Error(e, "ClearJob error");
        }
    }
}