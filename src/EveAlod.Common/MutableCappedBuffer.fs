namespace EveAlod.Common


type MutableCappedBuffer<'T> private (size, buffer: ResizeArray<'T>)=
    do if size <= 0 then invalidArg "size" "The size must be positive."
        
    new(size) = new MutableCappedBuffer<'T>(size, new ResizeArray<'T>(size))
    
    member this.Add(value: 'T)= 
        buffer.Insert(0, value)
        if(buffer.Count > size) then
            buffer.RemoveAt(buffer.Count - 1)
        this
               
    member __.Contains(value: 'T)= 
        buffer.Contains(value)

    member __.Items() = 
        seq {
            yield! buffer
        }

