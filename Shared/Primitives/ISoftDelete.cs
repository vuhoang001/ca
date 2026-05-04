namespace Auth.Shared.Primitives;

public interface ISoftDelete
{
    bool IsDeleted { get; set; }
    void Delete();

}
