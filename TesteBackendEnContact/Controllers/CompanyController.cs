using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TesteBackendEnContact.Core.Domain.ContactBook.Company;
using TesteBackendEnContact.Core.Interface.ContactBook.Company;
using TesteBackendEnContact.Repository.Interface;
using TesteBackendEnContact.Services;

namespace TesteBackendEnContact.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CompanyController : ControllerBase
    {
        private readonly ILogger<CompanyController> _logger;

        public CompanyController(ILogger<CompanyController> logger)
        {
            _logger = logger;
        }

        [HttpPost("Login")]
        [AllowAnonymous]
        public async Task<ActionResult<dynamic>> Authenticate([FromForm]int id, [FromServices] CompanyAuthService companyAuthService, [FromServices] ICompanyRepository companyRepository)
        {
            var company = await companyRepository.GetAsync(id);

            if (company == null)
            {
                _logger.LogInformation("Algo deu errado em alguma parte da criação da autenticação");
                return NotFound("Usuário não encontrado!");
            }
            var authService = companyAuthService.Authenticate(company);

            return Ok(authService);
        }

        // Deixei esse método apenas para teste da Autenticação
        // Já deixei criado a questão de autorização para administrador
        [HttpGet]
        [Route("authenticated")]
        [Authorize]
        public string Authenticated() => String.Format("Autenticado - {0}", User.Identity.Name);


        [HttpPost]
        public async Task<ActionResult<ICompany>> Post(ICompany company, [FromServices] ICompanyRepository companyRepository)
        {
            var post = await companyRepository.SaveAsync(company);

            if (post != null)
            {
                return Ok(post);
            }
            else
            {
                _logger.LogInformation("Algo deu errado com o banco, ou ContactBookId não confere!");
                return BadRequest();
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<ICompany>> Put(int id, Company company, [FromServices] ICompanyRepository companyRepository)
        {
            var edited = await companyRepository.EditAsync(id, company);

            if (edited != null)
            {
                return Ok(edited);
            }
            else
            {
                _logger.LogInformation("Algo deu errado com o banco, ou Id ou ContactBookId não conferem!");
                return BadRequest();
            }
        }

        [HttpDelete]
        public async Task<ActionResult> Delete(int id, [FromServices] ICompanyRepository companyRepository)
        {
            var delete = await companyRepository.DeleteAsync(id);

            if (delete)
            {
                return Ok("Companhia deletada com sucesso!");
            }
            else
            {
                _logger.LogInformation("Algo deu errado com o banco, ou Id não confere!");
                return BadRequest();
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ICompany>>> GetAll([FromServices] ICompanyRepository companyRepository)
        {
            var allList = await companyRepository.GetAllAsync();

            if (allList != null)
            {
                return Ok(allList);
            }
            else
            {
                _logger.LogInformation("Algo deu errado no acesso ao banco!");
                return BadRequest();
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ICompany>> Get(int id, [FromServices] ICompanyRepository companyRepository)
        {
            var company = await companyRepository.GetAsync(id);

            if (company != null)
            {
                return Ok(company);
            }
            else
            {
                _logger.LogInformation("Algo deu errado com o banco, ou Id inexistente!");
                return BadRequest();
            }
        }
    }
}
