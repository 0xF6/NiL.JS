async function a() {
    await $.delay(1000);
    return 'result of a';
}

async function b() {
    console.log(await a());
    console.log("----")
    console.log(await a());
    console.log("----")
    console.log(await a());
    console.log("----")
    console.log(await a());
    console.log("----")
    console.log(await a());
    console.log("----")
    console.log(await a());
    console.log("----")
    console.log(await a());
    return 'result of b';
}

b().then(console.log);
console.log("end");
$.sleep(10000);