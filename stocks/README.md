# Stock Quote Alert

Aplicação de console desenvolvida em C# para monitoramento contínuo da cotação de ativos da B3 e envio de alertas por e-mail com base em preços de referência definidos pelo usuário.

O sistema permite configurar um preço de referência para venda e outro para compra. Durante o monitoramento, a aplicação identifica quando a cotação entra em uma zona de compra ou venda e também quando retorna para a faixa normal, enviando notificações por e-mail de acordo com a mudança de estado.

## Funcionalidades

- Monitoramento contínuo da cotação de um ativo da B3.
- Consulta das cotações através da API da brapi.dev.
- Configuração do ativo e dos preços de referência através da linha de comando.
- Alerta por e-mail quando:
  - a cotação fica acima do preço de venda;
  - a cotação fica abaixo do preço de compra;
  - a cotação retorna para a faixa normal, encerrando uma oportunidade de compra ou venda.
- Controle de estado dos alertas para evitar o envio repetido do mesmo alerta.
- Configuração do servidor SMTP através de arquivo JSON.
- Encerramento seguro do monitoramento através de `Ctrl+C`.
- Validação dos parâmetros recebidos pela linha de comando.

## Tecnologias

- C#
- .NET 6
- MailKit
- MimeKit
- Microsoft.Extensions.Configuration
- Microsoft.Extensions.Configuration.Json
- Microsoft.Extensions.Configuration.Binder
- brapi.dev
- SMTP

## Arquitetura

O projeto foi organizado buscando separar as responsabilidades entre domínio, aplicação e infraestrutura.

```text
stocks/
├── Application/
│   ├── Interfaces/
│   │   ├── IEmailService.cs
│   │   └── IStockQuoteService.cs
│   ├── AlertStateEvaluator.cs
│   └── StockMonitoringService.cs
│
├── Configuration/
│   └── EmailConfiguration.cs
│
├── Domain/
│   ├── AlertState.cs
│   └── StockQuote.cs
│
├── Infrastructure/
│   ├── Email/
│   │   └── EmailService.cs
│   └── StockQuote/
│       └── StockQuoteService.cs
│
├── Program.cs
├── appsettings.example.json
├── appsettings.json
└── stocks.csproj
```

## Limitações da API

O projeto utiliza a API da brapi.dev para obter as cotações dos ativos.

A utilização da API está sujeita às limitações do plano utilizado. No plano gratuito:

- são permitidas 15.000 requisições por mês;
- cada requisição permite consultar 1 ticker;
- os tickers `PETR4`, `MGLU3`, `VALE3` e `ITUB4` podem ser consultados sem token;
- outros ativos exigem autenticação através de um token da API;
- nos planos Gratuito, Startup e Pro, o controle de uso considera a cota mensal e o número de requisições simultâneas, não havendo um limite geral fixo de requisições por segundo ou por minuto;
- o limite de 20 requisições por minuto por IP é específico do sandbox sem token.

A aplicação realiza uma consulta a cada 15 segundos. Isso representa até 4 requisições por minuto, ou aproximadamente 5.760 requisições por dia caso o programa permaneça em execução continuamente.

Portanto, o intervalo de 15 segundos foi definido para o funcionamento do projeto, mas a utilização contínua deve considerar a cota disponível da API.

## Como executar

### Pré-requisitos

- .NET 6 SDK instalado.
- Uma conta de e-mail com acesso SMTP.
- Para o Gmail, recomenda-se utilizar uma senha de aplicativo em vez da senha normal da conta.

É possível verificar a versão do .NET instalada com:

```bash
dotnet --version
```

### Configuração

Após clonar o repositório, entre na pasta do projeto:

```bash
git clone <URL_DO_REPOSITORIO>
cd DesafioTecnico/stocks
```
O arquivo appsettings.json contém as configurações necessárias para o envio dos e-mails e não é versionado no Git por conter informações sensíveis.Utilize o arquivo appsettings.example.json como modelo para criar o arquivo de configuração.

Em seguida, abra o arquivo appsettings.json e configure os dados do servidor SMTP, os campos possuem as seguintes finalidades:

- Recipient: endereço de e-mail que receberá os alertas.
- SmtpServer: endereço do servidor SMTP.
- SmtpPort: porta utilizada pelo servidor SMTP.
- Username: usuário utilizado para autenticação no servidor SMTP.
- Password: senha ou credencial utilizada para autenticação.

### Execução

O programa recebe três parâmetros pela linha de comando:

```bash
dotnet run -- <ativo> <preço-venda> <preço-compra>
```

Exemplo:
```bash
dotnet run -- PETR4 60 50
```

Nesse exemplo:

- PETR4 é o ativo monitorado;
- 60 é o preço de referência para venda;
- 50 é o preço de referência para compra.

Durante a execução, o programa consulta periodicamente a cotação do ativo e envia um e-mail quando ocorre uma mudança no estado do alerta.

Para encerrar o monitoramento, pressione: Ctrl + C

### Exemplo de Saída

```
Ativo: PETR4
Preço de venda: 60
Preço de compra: 50
Cotação atual: 49,47
Alerta: Cotação abaixo do preço de compra.
E-mail de compra enviado.
Cotação atual: 49,47
Cotação atual: 49,47
Monitoramento encerrado.
```

## Lógica dos alertas

A aplicação utiliza três estados para representar a situação atual do ativo:

- `Buy`: a cotação está abaixo do preço de compra definido;
- `Normal`: a cotação está entre os preços de compra e venda;
- `Sell`: a cotação está acima do preço de venda definido.

A cada consulta da cotação, o sistema avalia o preço atual e determina o novo estado do ativo.

Um novo e-mail é enviado somente quando ocorre uma mudança de estado. Dessa forma, o sistema evita o envio repetido do mesmo alerta enquanto a cotação permanece na mesma condição.

### Transições de estado
O estado inicial do monitoramento é `Normal`.

| Estado anterior | Novo estado | Ação |
|---|---|---|
| `Normal` | `Buy` | Envia alerta de compra |
| `Normal` | `Sell` | Envia alerta de venda |
| `Buy` | `Buy` | Nenhum e-mail |
| `Sell` | `Sell` | Nenhum e-mail |
| `Buy` | `Normal` | Envia aviso de encerramento da oportunidade de compra |
| `Sell` | `Normal` | Envia aviso de encerramento da oportunidade de venda |
| `Buy` | `Sell` | Envia novo alerta de venda |
| `Sell` | `Buy` | Envia novo alerta de compra |

Os limites são avaliados da seguinte forma:

```text
Cotação > preço de venda  → Sell
Cotação < preço de compra → Buy
Caso contrário             → Normal
```
Por exemplo, considerando um preço de venda de `R$ 60,00` e um preço de compra de `R$ 50,00`:

- Cotação de `R$ 65,00` → estado `Sell`;
- Cotação de `R$ 55,00` → estado `Normal`;
- Cotação de `R$ 45,00` → estado `Buy`.

O monitoramento é realizado continuamente, com uma nova consulta a cada 15 segundos, enquanto o programa estiver em execução.

## Uso de IA

Durante o desenvolvimento deste projeto foi utilizada a ferramenta ChatGPT (OpenAI) como apoio ao desenvolvimento.

A ferramenta foi utilizada principalmente para:

- discutir alternativas de implementação e organização do projeto;
- auxiliar na identificação e correção de erros;
- revisar trechos de código;
- discutir boas práticas de desenvolvimento;
- auxiliar na documentação do projeto.

As decisões técnicas, a implementação e a validação do funcionamento da aplicação foram realizadas e revisadas pelo autor.

A IA foi utilizada como ferramenta de apoio ao desenvolvimento, sem substituir a análise, a compreensão e as decisões técnicas necessárias para a construção da solução.