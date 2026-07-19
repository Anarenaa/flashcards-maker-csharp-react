using Core.Context;
using Core.Models;
using Repositories.Interfaces;

namespace Repositories
{
    public class FlashcardContextsRepository : Repository<FlashcardContext>, IFlashcardContextRepository
    {
        public FlashcardContextsRepository(BaseDataContext context) : base(context)
        {
        }
    }
}
