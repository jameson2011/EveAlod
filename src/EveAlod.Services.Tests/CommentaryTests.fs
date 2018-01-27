namespace EveAlod.Services.Tests

    open Xunit
    open FsCheck
    open FsCheck.Xunit
    open EveAlod.Data
    open EveAlod.Services

    module CommentaryTests=

        // TODO: what value???        
        let isTextTag t = Commentary.tags |> Seq.contains t
        let tagTextEqual tag = match isTextTag tag with
                                | true -> (<>)
                                | _ -> (=)

        [<Property(Verbose=true)>]
        let ``getTagText always produces non-empty text`` (tag: KillTag)=
            
            let r = Commentary.getTagText tag
            
            (tagTextEqual tag) r ""


        [<Property(Verbose=true)>]
        let ``getTagsText returns text``(tag: KillTag)=
            let tags = [ tag ]
            
            let r = tags |> Commentary.getTagsText Commentary.getTagText
            
            (tagTextEqual tag) r ""


