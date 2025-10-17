# 🧩 Sales & Stock Microservices  

Este projeto foi desenvolvido com foco em **boas práticas de arquitetura de software**, **mensageria assíncrona** e **resiliência transacional**, utilizando **.NET 8**, **C#** e **RabbitMQ**.  
Ele representa uma aplicação composta por dois microsserviços principais — **Sales** e **Stock** — que se comunicam de forma desacoplada por meio de **filas**.  

---

## 🚀 Tecnologias e Padrões Utilizados

- **.NET 8**  
- **C#**  
- **RabbitMQ** (mensageria assíncrona)  
- **SQL Server** (persistência de dados)  
- **Entity Framework Core / LINQ**  
- **MediatR** (desacoplamento via CQRS)  
- **FluentValidation** (validação das requisições)  
- **TransactionScope** (garantia ACID nas transações)  
- **Clean Architecture**  
- **SOLID Principles**  
- **CQRS Pattern**  
- **Middleware Global de Exceções**  
- **Testes Unitários**  
- **Logs Estruturados**

---

## 🏗️ Arquitetura da Aplicação

A aplicação segue os princípios da **Clean Architecture**, com separação clara entre **camadas de domínio, aplicação, infraestrutura e interface**.  
Isso garante **alta coesão, baixo acoplamento** e facilidade na **evolução e manutenção** do código.  

O fluxo entre os microsserviços funciona da seguinte forma:

1. O serviço **Sales** envia uma requisição de Validação de Estoque.  
2. O **RabbitMQ** recebe e encaminha a mensagem para a fila correspondente.  
3. O serviço **Stock** consome a mensagem, processa a validação do estoque e retorna a resposta via outra fila.  
4. O **Sales** valida a resposta e, sendo positiva, manda outra requisição para diminuição do Estoque.  
5. O serviço **Stock** consome a mensagem, processa p pedido de diminuição do estoque e retorna a resposta via outra fila.  
6. O serviço **Sales** valida a resposta, e, se positiva, conclui a venda.

Essa comunicação assíncrona assegura **escalabilidade, resiliência e isolamento entre serviços**.

---

## ⚙️ Recursos Implementados

### 🔹 CQRS + MediatR  
Separação entre **Commands** e **Queries**, garantindo clareza e isolamento das responsabilidades.

### 🔹 FluentValidation  
Validação das entradas de dados diretamente na camada de aplicação, antes da execução dos handlers.

### 🔹 TransactionScope  
Implementado no serviço de estoque para garantir **atomicidade** — em caso de falhas, todas as operações são revertidas, preservando a integridade dos dados.
Além de ter query otimizada para alteração de estoque em tempo de execução, para garantir que os dados estejam sempre consistentes, enquanto as alterações são assistidas pelo TransactionScope.

### 🔹 Middleware Global de Exceções  
Middleware responsável por capturar, registrar e padronizar os erros retornados pela API, oferecendo mensagens consistentes e facilitando o diagnóstico.


### 🔹 Testes Unitários  
Garantem o comportamento esperado dos principais serviços e regras de negócio, assegurando **confiabilidade e qualidade de código**.

### 🔹 Validação Inteligente de Mensagens  
O sistema valida as respostas das filas, garantindo que cada microsserviço processe apenas as mensagens correspondentes à sua própria requisição, evitando leituras incorretas.

---

##  Como Executar

### 📋 Pré-requisitos

- **.NET 8.0 SDK** ou superior
- **SQL Server** (LocalDB ou instância completa)
- **RabbitMQ** (local ou Docker)
- **Visual Studio 2022**, **VS Code** ou **Rider**
- **Docker** (opcional, para containerização)

### 🔧 Configuração e Execução Local

1. **Clone o repositório**
```bash
git clone <url-do-repositorio>
cd Microsservicos
```

2. **Configure o RabbitMQ**
```bash
# Opção 1: Docker (recomendado)
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management

# Opção 2: Instalação local
# Baixe e instale do site oficial: https://www.rabbitmq.com/download.html
```

3. **Configure as strings de conexão**
   - Atualize os arquivos `appsettings.json` em cada API
   - Configure as conexões com SQL Server e RabbitMQ

4. **Restaure as dependências**
```bash
dotnet restore MS-Sales/MS-Sales.sln
dotnet restore MS-Stock/MS-Stock.sln
```

5. **Execute as migrações do banco**
```bash
# Para MS-Sales
cd MS-Sales/Sales.Api
dotnet ef database update

# Para MS-Stock
cd ../../MS-Stock/Stock.Api
dotnet ef database update
```

6. **Execute os serviços** (em terminais separados)
```bash
# Terminal 1 - MS-Sales
cd MS-Sales/Sales.Api
dotnet run

# Terminal 2 - MS-Stock  
cd MS-Stock/Stock.Api
dotnet run
```

## 📚 Endpoints da API

### MS-Sales
- `POST /api/orders/CreateOrder` - Cria um novo pedido
- `GET /api/orders/{id}` - Obtém um pedido específico

### MS-Stock
- `GET /api/products/GetAllProducts` - Lista todos os produtos
- `POST /api/products/create` - Cria um novo produto
- `GET /api/products/GetProductById/{id}` - Obtém um produto específico
- `PUT /api/products/updateProduct` - Atualiza produto

## 🔧 Configuração

### 📡 Configuração do RabbitMQ

#### 🐳 Instalação via Docker (Recomendado)

```bash
# Executar RabbitMQ com Management UI
docker run -d --name rabbitmq \
  -p 5672:5672 \
  -p 15672:15672 \
  rabbitmq:3-management

# Verificar se está rodando
docker logs rabbitmq

# Acessar Management UI: http://localhost:15672
# User: guest | Pass: guest

```

#### 📋 Estrutura de Filas Criadas

| 🎯 **Fila** | 📝 **Descrição** | ⚡ **Durável** | 🔄 **Auto-Delete** |
|-------------|------------------|----------------|---------------------|
| `order.validation.stock.request` | Validação de estoque | ✅ | ❌ |
| `order.validation.stock.response` | Resposta da validação | ✅ | ❌ |
| `order.decrease.stock.request` | Atualização de estoque | ✅ | ❌ |
| `order.decrease.stock.response` | Resposta da atualização | ✅ | ❌ |

```

### 🌍 Variáveis de Ambiente

Crie arquivos `.env` ou configure as seguintes variáveis:

```env
# MS-Sales
ConnectionStrings__DefaultConnection=Server=localhost;Database=SalesDB;Trusted_Connection=true;

# MS-Stock
ConnectionStrings__DefaultConnection=Server=localhost;Database=StockDB;Trusted_Connection=true;
```


## 🧪 Executando Testes

```bash
# Testes do MS-Sales
cd MS-Sales/Sales.Tests
dotnet test

# Testes do MS-Stock
cd ../../MS-Stock/Stock.Tests
dotnet test
```






### 👨‍💻 **Desenvolvedor Principal**

**Matheus Maia Goulart**  
🏢 *Software Engineer @ Avanade*

[![LinkedIn](https://img.shields.io/badge/LinkedIn-0077B5?style=for-the-badge&logo=linkedin&logoColor=white)](https://linkedin.com/in/matheusmaiagoulart)
[![GitHub](https://img.shields.io/badge/GitHub-100000?style=for-the-badge&logo=github&logoColor=white)](https://github.com/matheusmaiagoulart)

---

### 💡 **Sobre o Projeto**

Este é um **projeto de estudos** desenvolvido para demonstrar:
- 🏗️ **Arquitetura de Microserviços** com .NET
- 📡 **Comunicação Assíncrona** via RabbitMQ  
- 🧅 **Clean Architecture** e princípios SOLID
- 🔄 **CQRS Pattern** com MediatR
- 🛡️ **Resiliência** e gerenciamento de transações

**⭐ Se este projeto foi útil para você, considere dar uma estrela!**

</div>

---

<div align="center">

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet)
![C#](https://img.shields.io/badge/C%23-239120?style=flat-square&logo=c-sharp&logoColor=white)
![RabbitMQ](https://img.shields.io/badge/RabbitMQ-FF6600?style=flat-square&logo=rabbitmq&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL%20Server-CC2927?style=flat-square&logo=microsoft-sql-server&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-2496ED?style=flat-square&logo=docker&logoColor=white)

**Made with ❤️ for the .NET Community**

</div>