$(async() =>
{
    try
    {
        let response = await fetch("http://localhost:5000/api/data")
        let json = await response.json()
        
        $.each(json, function(key, value)
        {
           $('#select').append($('<option/>',
           {
              text: "class: " + value.className + " count: " + value.count,
              value: value.className
            
           }))
        })

    }
    catch(e)
    {
        console.log(e)
    }
})
 

 




    
