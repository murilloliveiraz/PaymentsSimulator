using Bank.Context;
using BuildingBlocks.Core.DomainObjects;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using static Dapper.SqlMapper;

namespace Bank.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ApiController
    {
        private readonly BankContext _context;

        public AccountsController(BankContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetAccount([FromRoute] string id)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(acc => acc.Id == id);
            if (account is null)
                return CustomResponse((int)HttpStatusCode.NotFound, false);

            return CustomResponse((int)HttpStatusCode.OK, true, account);
        }

        [HttpPost("")]
        public async Task<ActionResult<Account>> CreateAccount([FromBody] string name)
        {
            var account = new Account
            {
                Name = name,
                Id = Guid.NewGuid().ToString()
            };

            await _context.Accounts.AddAsync(account);
            var response = await _context.SaveChangesAsync() > 0;

            if (!response)
                return CustomResponse((int)HttpStatusCode.BadRequest, false);

            return CustomResponse((int)HttpStatusCode.OK, true, account);
        }
    }
}
