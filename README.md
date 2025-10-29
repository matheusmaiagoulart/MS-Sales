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
- **YARP** (Gateway para roteamento de requisições aos microsserviços)  
* **2PC (Two-Phase Commit)**

  * **Fase 1:** Reserva de estoque (`ReservedQuantity`)
  * **Fase 2:** Confirmação ou Cancelamento da reserva
  * Garante atomicidade e consistência em cenários de alta concorrência
- **Clean Architecture**  
- **SOLID Principles**  
- **CQRS Pattern**
- **Background Service**
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
![system-design-microsservicos](https://github.com/user-attachments/assets/b15fb7f5-3d36-4bb2-87aa-8cee1b0e1b31)

---

## ⏱️ Background Service

Um **serviço em segundo plano** executa a cada **2 minutos** para:

* Identificar itens com status `Reserved` expirados;
* Reverter suas quantidades ao estoque;
* Atualizar o status para `Expired`.

---

## 🚪 Gateway com YARP e Autenticação JWT

* O projeto utiliza o **YARP** como gateway reverso para rotear requisições entre os microsserviços (Sales, Stock, etc).
* O gateway implementa autenticação básica via **JWT**. Para acessar as rotas protegidas, obtenha um token JWT através do endpoint de autenticação e inclua-o no header `Authorization: Bearer <token>` nas requisições.
* Exemplo de fluxo:
  1. Realize login via `/auth/login` no Gateway para obter o token JWT.
  2. Utilize o token para acessar rotas protegidas dos microsserviços via Gateway.

---

## 🧠 Arquitetura e Design

```text
Cliente → API Gateway → MS-Sales → RabbitMQ → MS-Stock → DB
                   ↘︎ Logs / Middlewares / Validations
```

Cada microsserviço é independente e se comunica via **filas assíncronas**, garantindo resiliência e desacoplamento entre os domínios.

---

## ✅ Padrões e Boas Práticas

* **Repository Pattern** 
* **Injeção de Dependências (DI)** nativa do .NET
* **Transações consistentes** e controle de concorrência
* **Testes unitários** cobrindo os fluxos críticos
* **Logs** 

---

## 🧪 Como Executar

1. Suba o RabbitMQ localmente (via Docker):

   ```bash
   docker run -d --hostname rabbit --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:management
   ```

2. Configure as variáveis de ambiente no `appsettings.json`.

3. Execute o projeto:

   ```bash
   dotnet run
   ```

4. Acesse o dashboard do RabbitMQ:

   ```
   http://localhost:15672
   ```

---

## 🧾 Logs e Monitoramento

* Logging via **Microsoft.Extensions.Logging**
* Middleware global para tratamento de exceções
* Registros de eventos de fila, falhas de transação e status de reserva

---

## 📄 Licença

Projeto desenvolvido para fins de estudo e demonstração.
**Autor:** Matheus Maia Goulart 🧑‍💻
🔗 [LinkedIn](https://www.linkedin.com/in/matheusmaiagoulart/)
