using MyAdvisor.Application.DTOs.RecurringTransaction;
using MyAdvisor.Application.Interfaces.Repositories;
using MyAdvisor.Application.Interfaces.Services.Domain;
using MyAdvisor.Application.Mappers;
using MyAdvisor.Domain.Entities;

namespace MyAdvisor.Application.Services
{
    public class RecurringTransactionService : IRecurringTransactionService
    {
        private readonly IRecurringTransactionRepository _repository;
        private readonly RecurringTransactionMapper _mapper;

        public RecurringTransactionService(
            IRecurringTransactionRepository repository,
            RecurringTransactionMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<RecurringTransactionDto> GetByIdAsync(int id, int userId)
        {
            var entity = await _repository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Recurring transaction {id} not found.");

            if (entity.UserId != userId)
                throw new UnauthorizedAccessException();

            return _mapper.ToDto(entity);
        }

        public async Task<IReadOnlyList<RecurringTransactionDto>> GetByUserIdAsync(int userId)
        {
            var entities = await _repository.GetByUserIdAsync(userId);
            return entities.Select(_mapper.ToDto).ToList();
        }

        public async Task<RecurringTransactionDto> CreateAsync(
            CreateRecurringTransactionRequestDto request,
            int userId)
        {
            var entity = new RecurringTransaction(
                userId,
                request.CategoryId,
                request.Amount,
                request.Frequency!.Value,
                request.PaymentMethod,
                request.NextDueDate,
                request.Description);

            await _repository.AddAsync(entity);
            return _mapper.ToDto(entity);
        }

        public async Task<RecurringTransactionDto> UpdateAsync(
            int id,
            UpdateRecurringTransactionRequestDto request,
            int userId)
        {
            var entity = await _repository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Recurring transaction {id} not found.");

            if (entity.UserId != userId)
                throw new UnauthorizedAccessException();

            entity.Update(
                request.Amount,
                request.CategoryId,
                request.Frequency!.Value,
                request.PaymentMethod,
                request.NextDueDate,
                request.Description);

            await _repository.UpdateAsync(entity);
            return _mapper.ToDto(entity);
        }

        public async Task DeleteAsync(int id, int userId)
        {
            var entity = await _repository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Recurring transaction {id} not found.");

            if (entity.UserId != userId)
                throw new UnauthorizedAccessException();

            await _repository.DeleteAsync(id);
        }
    }
}
