using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using TesteBackendEnContact.Core.Domain.ContactBook.Contact;
using TesteBackendEnContact.Core.Interface.ContactBook.Contact;
using TesteBackendEnContact.Repository.Interface;

namespace TesteBackendEnContact.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ContactController : ControllerBase
    {
        private readonly ILogger<ContactController> _logger;

        public ContactController(ILogger<ContactController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<IContact>> Create(Contact contact, [FromServices] IContactRepository contactRepository)
        {
            var createdContact = await contactRepository.SaveAsync(contact);

            if (createdContact != null)
            {
                return Ok(createdContact);
            }
            else
            {
                _logger.LogInformation("Algo deu errado com o banco, ou ContactBookId ou CompanyId não encontrados!");
                return BadRequest();
            }
        }

        [HttpPost("CSV")]
        public async Task<ActionResult<IEnumerable<IContact>>> CSVPost(string path, [FromServices] IContactRepository contactRepository)
        {
            var createdContact = await contactRepository.GetCSVFile(path);

            if (createdContact != null)
            {
                return Ok(createdContact);
            }
            else
            {
                _logger.LogInformation("Algo deu errado com o banco, ou ContactBookId ou CompanyId não encontrados!");
                return BadRequest();
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<IContact>> Update(int id, Contact contato, [FromServices] IContactRepository contactRepository)
        {
            var edited = await contactRepository.EditAsync(id, contato);

            if (edited == null)
            {
                _logger.LogInformation("Algo deu errado com a consulta no banco, ou não foi possível alterar esse Usuário!");
                return BadRequest();
            }
            else
            {
                return Ok(edited);
            }
        }

        [HttpDelete]
        public async Task<ActionResult> Delete(int id, [FromServices] IContactRepository contactRepository)
        {
            var deleted = await contactRepository.DeleteAsync(id);

            if (deleted)
            {
                return Ok(deleted);
            }
            else
            {
                _logger.LogInformation("Algo deu errado com a consulta no banco, ou Id inexistente!");
                return BadRequest();
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<IContact>>> GetAll([FromServices] IContactRepository contactRepository)
        {
            var allContacts = await contactRepository.GetAllAsync();

            if (allContacts != null)
            {
                return Ok(allContacts);
            }
            else
            {
                _logger.LogInformation("Algo deu errado com a consulta no banco, ou não há contatos cadastrados!");
                return NoContent();
            }
        }        

        [HttpGet("{id:int}")]
        public async Task<ActionResult<IContact>> Get(int id, [FromServices] IContactRepository contactRepository)
        {
            var displayContact = await contactRepository.GetAsync(id);

            if (displayContact != null)
            {
                return Ok(displayContact);
            }
            else
            {
                _logger.LogInformation("Algo deu errado com o banco, ou Id não encontrado!");
                return BadRequest();
            }
        }

        [HttpGet("{something}/skip/{skip:int}/take/{take:int}")]
        public async Task<ActionResult<dynamic>> GetAllOfSomething(string something, int skip, int take, [FromServices] IContactRepository contactRepository)
        {
            var listOfAll = await contactRepository.GetContacts(something, skip, take);

            if (listOfAll == null)
            {
                _logger.LogInformation("Algo deu errado com o banco!");
                return BadRequest();
            }
            else
            {
                return Ok(listOfAll);
            }
        }

        [HttpGet("ByCompany")]
        [AllowAnonymous]
        public ActionResult GetContactFromCompany([FromQuery]int companyId, [FromServices]IContactRepository contactRepository)
        {
            var listContacts = contactRepository.GetContactsByCompany(companyId);

            if (listContacts == null)
            {
                _logger.LogInformation("Algo deu errado com o banco, ou não existiam registros!");
                return BadRequest();
            }
            else
            {
                return Ok(listContacts);
            }
        }
    }
}
