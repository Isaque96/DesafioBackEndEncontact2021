using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using TesteBackendEnContact.Core.Domain.ContactBook;
using TesteBackendEnContact.Core.Interface.ContactBook;
using TesteBackendEnContact.Repository.Interface;

namespace TesteBackendEnContact.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ContactBookController : ControllerBase
    {
        private readonly ILogger<ContactBookController> _logger;

        public ContactBookController(ILogger<ContactBookController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<IContactBook>> Post(ContactBook contactBook, [FromServices] IContactBookRepository contactBookRepository)
        {
            if (!ModelState.IsValid || contactBook == null)
            {
                _logger.LogInformation("Preenchimento das Informações da Agenda estão incorretos!");
                return BadRequest();
            }

            var inserted = await contactBookRepository.SaveAsync(contactBook);

            if (inserted == null)
            {
                _logger.LogInformation("Algo deu errado com o banco ou com o método!");
                return BadRequest("Ops algo deu errado!");
            }

            return Ok(inserted);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<IContactBook>> Update(int id, ContactBook contactBook, [FromServices] IContactBookRepository contactBookRepository)
        {
            var edited = await contactBookRepository.EditAsync(id, contactBook);

            if (edited == null)
            {
                return BadRequest();
            }
            else
            {
                _logger.LogInformation("Algo deu errado com o banco, ou o não foi possível achar o Usuário!");
                return Ok(edited);
            }
        }

        [HttpDelete]
        public async Task<ActionResult> Delete(int id, [FromServices] IContactBookRepository contactBookRepository)
        {
            var deleted = await contactBookRepository.DeleteAsync(id);

            if (deleted)
            {
                return Ok("Agenda deletada!");
            }
            else
            {
                _logger.LogInformation("Algo deu errado com o banco, ou o Id não existia!");
                return BadRequest(); 
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<IContactBook>>> GetAll([FromServices] IContactBookRepository contactBookRepository)
        {
            var lista = await contactBookRepository.GetAllAsync();
            
            if (lista == null)
            {
                _logger.LogInformation("Algo deu errado com a consulta no banco, ou não há usuários agendas cadastradas!");
                return NoContent();
            }
            else
            {
                return Ok(lista);
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<IContactBook>> GetById(int id, [FromServices]IContactBookRepository contactBookRepository)
        {
            var contactBook = await contactBookRepository.GetAsync(id);

            if (contactBook == null)
            {
                _logger.LogInformation("Algo deu errado com a consulta no banco, ou não foi possível achar usuário com esse Id!");
                return NoContent();
            }

            return Ok(contactBook);
        }

        [HttpGet("ContactBookToCSV")]
        public async Task<ActionResult> GetCSVFromContactBook ([FromQuery]int id, [FromQuery]string path, [FromServices]IContactBookRepository contactBookRepository)
        {
            var created = await contactBookRepository.ContactsCSV(id, path);
            if (created)
            {
                return Ok(path);
            }
            else
            {
                _logger.LogInformation("Algo deu errado com o arquivo ou com a inserção no banco!");
                return BadRequest();
            }
        }
    }
}
