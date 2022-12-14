using Contracts.Account;
using Contracts.Common;
using Logic.Commands.Accounts;
using Logic.Queries.Accounts;

namespace ConsoleApp.Controllers
{
    public class AccountController : ConsoleController
    {
        private readonly ISender _mediator;
        private readonly IMapper _mapper;

        public AccountController(
            ISender mediator, 
            IMapper mapper
        )
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<object> Get(int id)
        {
            var result = await _mediator.Send(new GetAccountQuery(AccountId: id));

            return result.Match<object>(
                value => _mapper.Map<AccountResponse>(value),
                error => error
            );
        }

        public async Task<object> Delete(int id)
        {
            var command = _mapper.Map<DeleteAccountCommand>(id);
            var result = await _mediator.Send(command);

            return result.Match<object>(
                value => new CommandResponse(Result: value),
                error => error
            );
        }
    }
}