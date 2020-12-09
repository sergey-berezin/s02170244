var imgs = new Array()
var names = new Array()
var image = document.getElementById("image");
var text = document.getElementById('name');
flag = false
function slide_next()
{
  if (flag)
    {
      let current = +image.dataset.current;
      current += 1;
      if (current >= imgs.length)
        current = 0;
      
      image.src = imgs[current];
      text.textContent = names[current];
      image.dataset.current = current;
    }
}
function slide_prev()
{
  if (flag)
  { 
    let current = +image.dataset.current;
    current += -1;
    if (current < 0)
        current = imgs.length - 1;
    
    image.src = imgs[current];
    text.textContent = names[current];
    image.dataset.current = current;
  }
}

async function changeOption()
{
    var select = document.getElementById("select")
    var selectedOption = select.options[select.selectedIndex]
    //console.log(selectedOption.value)
    try
    {
        let response = await fetch("http://localhost:5000/api/data/" + selectedOption.value)
        let json = await response.json()
        
        imgs = new Array()
        names = new Array()
        
        $.each(json, function(key, value)
        {
            imgs.push("data:image/jpg;base64," + value.image)
            names.push(value.name)
        })

        flag = true
        image.src = imgs[0];
        text.textContent = names[0];
        image.dataset.current = 0;

       
        

    }
    catch(e)
    {
        console.log(e)
    }

}
