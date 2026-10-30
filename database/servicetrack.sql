-- Cria a base de dados.
CREATE DATABASE IF NOT EXISTS servicetrack
CHARACTER SET utf8mb4
COLLATE utf8mb4_unicode_ci;

USE servicetrack;

-- Cria a única tabela da aplicação.
CREATE TABLE IF NOT EXISTS pedidos (
    id INT PRIMARY KEY AUTO_INCREMENT,
    titulo VARCHAR(255) NOT NULL,
    descricao TEXT NOT NULL,
    nome_contato VARCHAR(255) NOT NULL,
    email_contato VARCHAR(255) NOT NULL,
    categoria VARCHAR(50) NOT NULL,
    prioridade VARCHAR(20) NOT NULL DEFAULT 'Normal',
    estado VARCHAR(20) NOT NULL DEFAULT 'Aberto',
    criado_em DATETIME NOT NULL,
    atualizado_em DATETIME NOT NULL,
    resolvido_em DATETIME NULL
);
