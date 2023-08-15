let source: EventSource;

const printLogs = () => {
    const textarea = document.querySelector("textarea");
    source = new EventSource('/api/logs/stream');

    if(!textarea){
        return;
    }
    
    source.onmessage = (event) => {
        console.log(event.data);
        setTimeout(() => {
            textarea.value += (event.data + "\n");
            textarea.scrollTop = textarea.scrollHeight;
        });
    };

    source.onopen = (event) => {
        console.log('onopen');
    };

    source.onerror = (event) => {
        console.log('onerror');
    }

}

const restart = async () => {
    source.close();
    await fetch("/api/restart", {method: "POST"}).catch(console.error);
    alert("Container restarted !");
    printLogs()
}

const update = async () => {
    source.close();
    await fetch("/api/update", {method: "POST"});
    alert("Container updated !");
    printLogs()
}