using Logic.AccountManager.Common;
using Logic.AccountManager.Commands;
using Contracts.AccountManager;

namespace ConsoleApp.Common.Mapping
{
    public class MappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<RegisterRequest, RegisterCommand>()
                .Map(dest => dest.User, src => src.User);

            config.NewConfig<AccountManagerResult, AccountManagerResponse>()
                .Map(dest => dest, src => src.Account);
        }
    }
}
