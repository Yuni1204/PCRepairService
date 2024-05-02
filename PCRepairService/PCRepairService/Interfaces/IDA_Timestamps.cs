namespace PCRepairService.Interfaces
{
    public interface IDA_Timestamps
    {
        Task AddAsync(string timestampstr, long id);
    }
}
