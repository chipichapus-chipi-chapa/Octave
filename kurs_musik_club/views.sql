-- Создание представления для отображения услуг и их стоимости
CREATE OR REPLACE VIEW public.services_with_costs AS
SELECT 
    s.id_service,
    s.name_service,
    c.cost_hour as price
FROM 
    public.services s
JOIN 
    public.cost c ON s.id_cost = c.id_cost;

-- Добавление комментария к представлению
COMMENT ON VIEW public.services_with_costs IS 'Представление для отображения услуг и их стоимости';

-- Предоставление прав на использование представления
GRANT SELECT ON public.services_with_costs TO postgres; 