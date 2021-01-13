# Contents
* A basic project to show the Redux lifecycle

* The example of people buying insurance policies and making claims from a company is used here

* Redux is great for complex applications, and can be used outside of React

# Redux - Using an example of insurance company
To show how Redux helps in creating a web app, visualize customers going to a company's branch office to submit orders.

Using insurance companies as an example, customers can buy, make claims and terminate their policies. All these actions take place at the branch, and the branch works as a dispatch for submitting the actions.

## Diagrams
Diagram for Redux lifecycle

![Redux lifecycle](https://github.com/FadeDragon/Resume2020/blob/master/Redux%20-%20Insurance%20company%20example/Redux%20lifecycle.svg)

How it matches the example of insurance company

![Example](https://github.com/FadeDragon/Resume2020/blob/master/Redux%20-%20Insurance%20company%20example/Example.svg)

* An action is created and the action object is dispatched.
* Each reducer will get a copy of the action, and the state.
  * claimsHistory will keep track of claims and take no action on other actions.
  * accounting works to add and deduct money from the company balance.
  * policies keep track of policies and take no action on claims action.
* At any time, calling store.getState() allows the company to get an overview of the current state.

## Usage proposal.

### createPolicy
A customer buying an insurance policy. The UI calls this to store the new policy.

* name - Identifies the customer.
* amount - The money to purchase the policy.

Upon reaching the reducers, the state will have the new policy included.

### deletePolicy
A customer terminating or withdraws their policy. The UI calls this to remove the policy.

* name - Identifies the customer.

### createClaim
A customer has something bad happen to them. The UI calls this to payout to the customer.

* name - Identifies the customer.
* amountOfMoneyToCollect - The amount of money to claim.

Upon reaching the reducers, the state will have the amount of money deducted and a new entry added to claims history.

## Next steps.

With Redux managing the state, we make use of React hooks to implement Redux in a React application.

Multiple components can now share state using Redux and reducers ensure future code changes can modify the state of the application via reducers.
