using Xunit;
using TesteBackendEnContact.Repository;
using System.Threading.Tasks;

namespace EnContactTest
{
    public class ContactRepositoryTest : IClassFixture<TestDbFixture>
    {
        readonly TestDbFixture fixture = new();

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async Task ReceberTodosContatosDeUmaEmpresa_NãoVoltaNulo(int id)
        {
            // Arrange
            var context = fixture.Db;
            
            // Act
            var repository = new ContactRepository(context);

            var result = await repository.GetContactsByCompany(id);
            
            // Assert
            Assert.NotNull(result);
            fixture.Dispose();
        }

        [Fact]
        public async Task LerArquivoCSVESalvarNoBanco_NãoVoltaNulo()
        {
            // Arrange
            var context = fixture.Db;

            // Act
            var repository = new ContactRepository(context);

            var result = await repository.GetCSVFile(@"D:\Desktop\Teste\DesafioBackEndEncontact2021\testsContact.csv");

            // Assert
            Assert.NotNull(result);
            fixture.Dispose();
        }

        [Theory]
        [InlineData("Company", 0, 2)]
        [InlineData("teste", 0, 2)]
        public async Task PesquisarPorNomeEmQualquerCampo_NãoVoltaNulo(string something, int skip, int take)
        {
            // Arrange
            var context = fixture.Db;

            // Act
            var repository = new ContactRepository(context);

            var result = await repository.GetContacts(something, skip, take);

            // Assert
            Assert.NotNull(result);
            fixture.Dispose();
        }
    }
}
