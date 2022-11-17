document.getElementById("myButton").addEventListener("click", myFunction);

function myFunction(){
  console.log('clicked my button');
  var newURL = "https://youshallnotpass.org";
  chrome.tabs.create({ url: newURL });
  // fetch('https://youshallnotpassauthtest2.azurewebsites.net/', {mode: 'no-cors'})
  // .then((response) => response.json())
  // .then((data) => console.log(data))
  // .catch(() => {console.log("Unexpected error sending email.")});
}