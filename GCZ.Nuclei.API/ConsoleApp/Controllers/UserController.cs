using Contracts.Common;
using Contracts.User;
using Logic.Commands.Users;
using Logic.Queries.Users;

namespace ConsoleApp.Controllers
{
    public class UserController : ConsoleController
    {
        private readonly ISender _mediator;
        private readonly IMapper _mapper;

        public UserController(
            ISender mediator,
            IMapper mapper
        )
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<object> Insert(InsertUserRequest request)
        {
            var command = _mapper.Map<InsertUserCommand>(request);
            var result = await _mediator.Send(command);

            return result.Match<object>(
                value => new CommandResponse(Result: value),
                error => error
            );
        }

        public async Task<object> Get(int id)
        {
            var result = await _mediator.Send(new GetUserQuery(UserId: id));

            return result.Match<object>(
                value => _mapper.Map<UserResponse>(value),
                error => error
            );
        }

        public async Task<object> GetAll()
        {
            var result = await _mediator.Send(new GetAllUsersQuery());

            return result.Match<object>(
                value => _mapper.Map<List<UserResponse>>(value),
                error => error
            );
        }

        public async Task<object> Update(UpdateUserRequest request)
        {
            var command = _mapper.Map<UpdateUserCommand>(request);
            var result = await _mediator.Send(command);

            return result.Match<object>(
                value => new CommandResponse(Result: value),
                error => error
            );
        }

        public async Task<object> Delete(int id)
        {
            var command = _mapper.Map<DeleteUserCommand>(id);
            var result = await _mediator.Send(command);

            return result.Match<object>(
                value => new CommandResponse(Result: value),
                error => error
            );
        }
    }
}
