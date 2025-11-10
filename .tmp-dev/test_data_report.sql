INSERT INTO "Table"(table_code) VALUES 
('table-1'),
('table-2'),
('table-3');


INSERT INTO "Order"(order_code, created_at, amount_clients,
"WaiterId", "TableId", "ChefId", status, cooking_status) VALUES 
('code-1', now(), 1, 1, 1, 1, 1, false),
('code-2', now(), 1, 1, 3, 1, 1, true),
('code-3', now(), 1, 1, 2, 1, 1, false);

