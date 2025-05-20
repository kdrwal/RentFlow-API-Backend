using RentFlow.Models;

namespace RentFlow.Repositories
{
    public interface IBikeRepository
    {
        Task <IEnumerable<Bike>> GetAllAsync();
        Task<Bike> GetByIdAsync(int id);

        Task AddBikeAsync(Bike bike);
        Task UpdateBikeAsync(Bike bike);
        Task DeleteBikeAsync(int id);


        
    }
}
