document.getElementById("myButton").addEventListener("click", myFunction);

function myFunction(){
  console.log('clicked my button');
  fetch('http://127.0.0.1:5000/sendmail', {mode: 'no-cors'})
  .then((response) => response.json())
  .then((data) => console.log(data))
  .catch(() => {console.log("Unexpected error sending email.")});
}