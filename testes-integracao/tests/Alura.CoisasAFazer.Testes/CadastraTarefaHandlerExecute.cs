using Alura.CoisasAFazer.Core.Commands;
using Alura.CoisasAFazer.Core.Models;
using Alura.CoisasAFazer.Infrastructure;
using Alura.CoisasAFazer.Services.Handlers;
using Alura.CoisasAFazer.Testes.TestDubles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Linq;
using Xunit;

namespace Alura.CoisasAFazer.Testes
{
    public class CadastraTarefaHandlerExecute
    {
        [Fact]
        public void QuandoSQLExceptionEhLancadaDeveComunicarResultadoNoComando()
        {
            //arrange
            var comando = new CadastraTarefa("Estudar Xunit", new Categoria("Estudo"), new DateTime(2019, 12, 31));
            //setup do dubl�
            var mock = new Mock<IRepositorioTarefas>();
            var repo = mock.Object;
            //como configurar o lan�amento da exce��o? no pr�prio teste!
            mock.Setup(r => r.IncluirTarefas(It.IsAny<Tarefa[]>())).Throws(new Exception("Houve um erro na inclus�o..."));

            var logger = new Mock<ILogger<CadastraTarefaHandler>>().Object;

            var handler = new CadastraTarefaHandler(repo, logger);

            //act: mudan�a no design da solu��o! TDD
            CommandResult resultado = handler.Execute(comando);

            //assert
            Assert.False(resultado.Success);
        }

        [Fact]
        public void DadaTarefaComInformacoesValidasDeveIncluirNoRepositorio()
        {
            //arrange
            var comando = new CadastraTarefa("Estudar Xunit", new Categoria("Estudo"), new DateTime(2019, 12, 31));
            //setup do dubl�
            var repo = new RepoFakeTarefas();
            var mock = new Mock<ILogger<CadastraTarefaHandler>>();
            var logger = mock.Object;

            var handler = new CadastraTarefaHandler(repo, logger);

            //act
            var resultado = handler.Execute(comando);

            //assert
            Assert.True(resultado.Success);
            var tarefa = repo.ObtemTarefas(t => t.Categoria.Descricao == "Estudo").FirstOrDefault();
            Assert.NotNull(tarefa);
            Assert.Equal("Estudar Xunit", tarefa.Titulo);
            Assert.Equal(new DateTime(2019, 12, 31), tarefa.Prazo);
        }

        [Fact]
        public void DadaTarefaComInformacoesValidasDeveIncluirNoRepositorio_InMemoryDatabase()
        {
            //arrange
            var comando = new CadastraTarefa("Estudar Xunit", new Categoria("Estudo"), new DateTime(2019, 12, 31));

            //setup do dubl�
            var options = new DbContextOptionsBuilder<DbTarefasContext>()
                .UseInMemoryDatabase("Teste de Integra��o")
                .Options;
            var contexto = new DbTarefasContext(options);
            var repo = new RepositorioTarefa(contexto);

            var mock = new Mock<ILogger<CadastraTarefaHandler>>();
            var logger = mock.Object;

            var handler = new CadastraTarefaHandler(repo, logger);

            //act
            handler.Execute(comando);

            //assert
            var tarefa = repo.ObtemTarefas(t => t.Categoria.Descricao == "Estudo").FirstOrDefault();
            Assert.NotNull(tarefa);
            Assert.Equal("Estudar Xunit", tarefa.Titulo);
            Assert.Equal(new DateTime(2019, 12, 31), tarefa.Prazo);
        }

        delegate void CaptureLogMessage(LogLevel l, EventId id, object o, Exception e, Func<object, Exception, string> func);

        [Fact]
        public void DadaTarefaComInformacoesValidasDeveLogarAOperacao()
        {
            //arrange
            var comando = new CadastraTarefa("Estudar Xunit", new Categoria("Estudo"), new DateTime(2019, 12, 31));

            //setup dos dubl�s
            var options = new DbContextOptionsBuilder<DbTarefasContext>()
                .UseInMemoryDatabase("Teste de Integra��o")
                .Options;
            var contexto = new DbTarefasContext(options);
            var repo = new RepositorioTarefa(contexto);

            var mock = new Mock<ILogger<CadastraTarefaHandler>>();

            string logOutput = string.Empty;
            CaptureLogMessage capture = (l, i, v, e, f) =>
            {
                logOutput = logOutput + v.ToString();
            };

            mock.Setup(x => x.Log(LogLevel.Debug, It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>())).Callback(capture);
            var logger = mock.Object;

            var handler = new CadastraTarefaHandler(repo, logger);

            //act
            handler.Execute(comando);

            //assert
            //COMO VERIFICAR SE O LOG FOI REALIZADO?
            Assert.Contains("Persistindo a tarefa", logOutput);
        }

        [Fact]
        public void QuandoExceptionForLancadaDeveLogarAMensagemDaExcecao()
        {
            //arrange
            var mensagemErro = "Houve um erro na inclus�o...";
            var excecaoEsperada = new Exception(mensagemErro);
            var comando = new CadastraTarefa("Estudar Xunit", new Categoria("Estudo"), new DateTime(2019, 12, 31));
            //setup do dubl�
            var mock = new Mock<IRepositorioTarefas>();
            var repo = mock.Object;
            //como configurar o lan�amento da exce��o? no pr�prio teste!
            mock.Setup(r => r.IncluirTarefas(It.IsAny<Tarefa[]>())).Throws(excecaoEsperada);

            var logger = new Mock<ILogger<CadastraTarefaHandler>>();

            var handler = new CadastraTarefaHandler(repo, logger.Object);

            //act: mudan�a no design da solu��o! TDD
            CommandResult resultado = handler.Execute(comando);

            //assert
            logger.Verify(l =>
                l.Log(
                    LogLevel.Error, //n�vel de log => LogError
                    It.IsAny<EventId>(), //identificador do evento
                    It.IsAny<object>(), //objeto que ser� logado
                    excecaoEsperada, // exce��o que ser� logada
                    It.IsAny<Func<object, Exception, string>>() //fun��o que converte objeto e a exce��o em uma string
                ), Times.Once());
        }

        delegate void CapturaMensagemLog(LogLevel level, EventId eventId, object state, Exception exception, Func<object, Exception, string> function);

        [Fact]
        public void DadaTarefaComInformacoesValidasDeveLogar()
        {
            //arrange
            var tituloTarefaEsperado = "Usar Moq para aprofundar conhecimento API";
            var comando = new CadastraTarefa(tituloTarefaEsperado, new Categoria(100, "Estudo"), new DateTime(2019, 12, 31));

            var mockLogger = new Mock<ILogger<CadastraTarefaHandler>>();

            LogLevel levelCapturado = LogLevel.Error;
            string mensagemCapturada = string.Empty;

            CapturaMensagemLog captura = (level, eventId, state, exception, func) =>
            {
                levelCapturado = level;
                mensagemCapturada = func(state, exception);
            };

            mockLogger.Setup(l =>
                l.Log(
                    It.IsAny<LogLevel>(), //n�vel de log => LogError
                    It.IsAny<EventId>(), //identificador do evento
                    It.IsAny<object>(), //objeto que ser� logado
                    It.IsAny<Exception>(), // exce��o que ser� logada
                    It.IsAny<Func<object, Exception, string>>() //fun��o que converte objeto e a exce��o em uma string
                )).Callback(captura);

            var mock = new Mock<IRepositorioTarefas>();


            var handler = new CadastraTarefaHandler(mock.Object, mockLogger.Object);

            //act
            handler.Execute(comando);

            //assert
            Assert.Equal(LogLevel.Debug, levelCapturado);
            Assert.Contains(tituloTarefaEsperado, mensagemCapturada);
        }
    }
}
