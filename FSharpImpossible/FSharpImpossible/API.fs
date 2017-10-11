namespace Impossible

module API =

    open Newtonsoft.Json.Linq
    open System.Collections.Generic

    type FilmList = {
        ID : int
        Title : string
        Description : string
        Actors : JRaw // {id : int; name : string}[]
    }

    type Film = {
        ID : int
        Title : string
        Description : string
        Category : string
        Price : System.Decimal
        Length : int
        Rating : string
        Actors : string[]
    }

    type ActorList = {
        ID : int
        FirstName : string
        LastName : string
    }

    type FilmTitle = {
        ID : int
        Title : string
    }

    type Actor = {
        ID : int
        FirstName : string
        LastName : string
        Films : FilmTitle[]
    }

    type Version =
        | V1P0
        | V1P1
        override this.ToString () =
            match this with
            | V1P0 -> ""
            | V1P1 -> "1-1"

    type URL =
        | URL of url : string
        | URLAndVersion of url : string * Version

    type APIList =
        | Films of  pageNumber : int * version : Version option
        | Actors of pageNumber : int
        | Actor of id : int
        | Film of  id : int

        member this.URL () =
            match this with
            | Films (pageNumber, version) -> sprintf "/films/%O?pageNumber=%i" version pageNumber
            | Actors pageNumber -> sprintf "/actors?pageNumber=%i" pageNumber
            | Actor id -> sprintf "/actors/%i" id
            | Film id -> sprintf "/films/%i" id

