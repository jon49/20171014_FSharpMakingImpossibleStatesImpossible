namespace Impossible

module DB =

    open System.Linq
    open FSharp.Data
    open FSharpx.Control
    open Newtonsoft.Json.Linq
    open Newtonsoft.Json
    open API
    open System.Data.SqlClient
    open Newtonsoft.Json.Serialization

    [<Literal>]
    let ConnectionString =
        "Data Source=.;Initial Catalog=sakila;MultipleActiveResultSets=true;Integrated Security=True"

    let private tuple x y = x, y

    let private serialize xs =
        let settings = new JsonSerializerSettings ()
        settings.ContractResolver <- new CamelCasePropertyNamesContractResolver()
        settings.NullValueHandling <- NullValueHandling.Ignore
        JsonConvert.SerializeObject(xs, settings)

    [<Literal>]
    let MoviePageSize = 10

    let getMovieListPageCount (conn : SqlConnection) =
        use movieCountCtx = new SqlCommandProvider<"
            SELECT COUNT(*)
            FROM dbo.film
            ", ConnectionString>(conn)
        movieCountCtx.Execute()
        |> fun xs -> (Option.orElse 0 (xs.First()))/MoviePageSize
        
    let getMovieList pageNumber version (conn : SqlConnection) =
        use movieListCtx = new SqlCommandProvider<"
            DECLARE @PageSize_ int = @PageSize;
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
            OFFSET @PageSize_ * (@PageNumber - 1) ROWS
            FETCH NEXT @PageSize_ ROWS ONLY;
            ", ConnectionString>(conn)
        movieListCtx.AsyncExecute(MoviePageSize, pageNumber)
        |> Async.Catch
        |> Async.map (
            function
            | Choice2Of2 (:?System.Data.SqlClient.SqlException as exn) -> failwithf "PageNumber: %i\n%s" pageNumber exn.Message
            | Choice2Of2 exn -> raise exn
            | Choice1Of2 xs ->
                match version with
                | V1P0 ->
                        xs
                        |> Seq.map ( fun x ->
                            {
                                FilmList.ID = x.film_id
                                Title = x.title
                                Description = Option.orElse "" x.description
                                Actors = null
                            } )
                | V1P1 ->
                        xs
                        |> Seq.map ( fun x ->
                            let actors = JRaw(Option.orElse "[]" x.actors)
                            {
                                FilmList.ID = x.film_id
                                Title = x.title
                                Description = Option.orElse "" x.description
                                Actors = actors
                            } )
                    )
                |> serialize |> tuple <| Films(pageNumber, Some version)

