a = 2;
console.log(a);
const a = 1
console.log(a);
var result;

function* igen() {
    for (var i = 0; i < 5; i++)
        yield i;
    return 10;
}

function* gen() { result = yield* igen(); }

for (var g of gen())
    console.log(JSON.stringify(g));

console.log(result);