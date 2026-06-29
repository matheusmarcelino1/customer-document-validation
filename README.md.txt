# Customer Document Validation

Plataforma backend para recebimento, armazenamento e processamento assíncrono de documentos cadastrais.

## Objetivo

Construir uma solução baseada em .NET, MongoDB, S3, Kafka e Worker Service para receber dados cadastrais, armazenar documentos, publicar eventos e processar documentos de forma assíncrona.

## Arquitetura Inicial

- CustomerDocumentValidation.Api
- CustomerDocumentValidation.Application
- CustomerDocumentValidation.Domain
- CustomerDocumentValidation.Infrastructure
- CustomerDocumentValidation.Worker
- CustomerDocumentValidation.SharedKernel

## Fluxo Planejado

1. API recebe dados cadastrais e documento.
2. Documento é salvo no S3.
3. Dados cadastrais são salvos no MongoDB.
4. API publica evento no Kafka.
5. Worker consome o evento.
6. Worker busca documento no S3.
7. Worker atualiza status no MongoDB.

## Tecnologias

- .NET 10
- ASP.NET Core
- C#
- MongoDB
- Kafka
- S3
- Worker Service
- xUnit
- Docker

## Como executar

Em construção.

## Status

Estrutura inicial criada.