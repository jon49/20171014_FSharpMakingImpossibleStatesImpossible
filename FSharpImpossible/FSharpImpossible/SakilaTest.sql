
DECLARE
  @PageSize int = 10
, @PageNumber int = 2

SELECT *
FROM dbo.film_list t
ORDER BY t.title ASC
OFFSET @PageSize * (@PageNumber - 1) ROWS
FETCH NEXT @PageSize ROWS ONLY;

SELECT
    t.film_id
  , t.title
  , t.[description]
  , (
    SELECT a.actor_id id, CONCAT(a.first_name, a.last_name) [name]
    FROM dbo.film_actor fa
    JOIN dbo.actor a
        ON a.actor_id = fa.actor_id
    WHERE fa.film_id = t.film_id
    FOR JSON AUTO
    ) actors
FROM dbo.film t
ORDER BY t.title ASC
OFFSET @PageSize * (@PageNumber - 1) ROWS
FETCH NEXT @PageSize ROWS ONLY;

SELECT COUNT(*)
FROM dbo.film t



SELECT *
FROM dbo.actor t
