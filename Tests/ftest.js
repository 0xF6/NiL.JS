class BaseClass {
	constructor(name) {
		this.name = name;
	}

    sayHello() {
        console.log("Hello, " + this.name + "! I'm Base");
    }

	static sayHello() {
		console.log("Hello everyone!")
	}
}

class DerivedClass extends BaseClass {
    sayHello() {
        console.log("Hi, " + this.name + "! I'm Derived");
        super.sayHello();
		DerivedClass.sayHello();
    }

	static sayHello() {
		console.log("Hello everyone! I'm Derived");
		super.sayHello();
	}
}

var instance = new DerivedClass("developer");
instance.sayHello();