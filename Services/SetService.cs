using Core.Models;
using Repositories.Interfaces;

namespace Services
{
    public class SetService
    {
        public readonly IUnitOfWork _unitOfWork;
        public SetService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<List<Set>> GetAllSetsAsync(string? searchText = null)
        {
            var sets = await _unitOfWork.Sets.GetAllAsync(
                    filter: s => s.IsPublic
                    && ((string.IsNullOrEmpty(searchText) 
                            || s.Name.ToLower().Contains(searchText.ToLower())
                        ))
            );
            return sets.ToList();
        }
        public async Task<List<Set>> GetAllUserSets(int userId, string? searchText = null)
        {
            var sets = await _unitOfWork.Sets.GetAllAsync(
                    filter: s => s.UserId == userId
                    && ((string.IsNullOrEmpty(searchText) 
                        || s.Name.Contains(searchText) 
                        || s.Description.Contains(searchText)))
            );
            return sets.ToList();
        }
        public async Task<Set?> GetSetByIdAsync(int id)
        {
            var set = await _unitOfWork.Sets.GetByIdAsync(id, "Flashcards");
            return set;
        }

        //додати метод "додавання сету до колекції"

        public async Task AddSetAsync(Set set)
        {
            await _unitOfWork.Sets.AddAsync(set);
            await _unitOfWork.SaveChangesAsync();
        }
        public async Task UpdateSetAsync(Set set)
        {
            await _unitOfWork.Sets.UpdateSetAsync(set);
            await _unitOfWork.SaveChangesAsync();
        }
        public async Task DeleteSetAsync(int id)
        {
            var set = await _unitOfWork.Sets.GetByIdAsync(id);
            if(set is not null)
            {
                _unitOfWork.Sets.Delete(set);
                await _unitOfWork.SaveChangesAsync();
            }
        }
    }
}
