using FUDTOs;

namespace Services
{
    public interface ISystemAccountService
    {
        Task<IEnumerable<SystemAccountDTO>> GetAccounts();
        Task<SystemAccountDTO> GetAccountById(short id);
        Task Create(SystemAccountDTO account);
        Task Update(SystemAccountDTO account);
        /*Task Delete(short id);*/
        Task<HttpResponseMessage> Delete(short id);
        Task<LoginResponseDTO?> Login(LoginRequestDTO loginDto);
    }
}
