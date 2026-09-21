-- Создание таблицы для бронирований зала
CREATE TABLE IF NOT EXISTS hall_rentals (
    id_rental SERIAL PRIMARY KEY,
    id_hall INTEGER NOT NULL,
    rental_date DATE NOT NULL,
    start_time TIME NOT NULL,
    end_time TIME NOT NULL,
    id_client INTEGER,
    rental_purpose VARCHAR(255),
    rental_status VARCHAR(50) DEFAULT 'active',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (id_hall) REFERENCES halls(id_hall),
    FOREIGN KEY (id_client) REFERENCES clients(id_client)
);

-- Создание индекса для быстрого поиска по дате и залу
CREATE INDEX idx_hall_rentals_date_hall ON hall_rentals(rental_date, id_hall);

-- Создание функции для проверки доступности зала
CREATE OR REPLACE FUNCTION check_hall_availability(
    p_hall_id INTEGER,
    p_rental_date DATE,
    p_start_time TIME,
    p_end_time TIME
) RETURNS BOOLEAN AS $$
BEGIN
    -- Проверяем, есть ли пересекающиеся бронирования
    RETURN NOT EXISTS (
        SELECT 1 FROM hall_rentals
        WHERE id_hall = p_hall_id
        AND rental_date = p_rental_date
        AND rental_status = 'active'
        AND (
            (start_time <= p_start_time AND end_time > p_start_time)
            OR (start_time < p_end_time AND end_time >= p_end_time)
            OR (start_time >= p_start_time AND end_time <= p_end_time)
        )
    );
END;
$$ LANGUAGE plpgsql;

-- Создание триггера для проверки доступности перед вставкой
CREATE OR REPLACE FUNCTION check_hall_availability_trigger()
RETURNS TRIGGER AS $$
BEGIN
    IF NOT check_hall_availability(
        NEW.id_hall,
        NEW.rental_date,
        NEW.start_time,
        NEW.end_time
    ) THEN
        RAISE EXCEPTION 'Зал уже забронирован на это время';
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER check_hall_availability_before_insert
    BEFORE INSERT ON hall_rentals
    FOR EACH ROW
    EXECUTE FUNCTION check_hall_availability_trigger(); 