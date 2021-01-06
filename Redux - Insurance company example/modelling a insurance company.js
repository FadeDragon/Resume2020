// Action Creation
const createPolicy = (name, amount) => {
  return { // Action (a form in our analogy)
    type: 'CREATE_POLICY',
    payload: {
      name: name,
      amount: amount
    }
  };
};

const deletePolicy = (name) => {
  return {
    type: 'DELETE_POLICY',
    payload: {
      name: name
    }
  };
};

const createClaim = (name, amountOfMoneyToCollect) => {
  return {
    type: 'CREATE_CLAIM',
    payload: {
      name: name,
      amountOfMoneyToCollect: amountOfMoneyToCollect
    }
  };
};


// Reducers (Departments in a company)
const claimsHistory = (oldListOfClaims = [], action) => {
  if (action.type === 'CREATE_CLAIM') {
    // want this action
    return [...oldListOfClaims, action.payload];
  }
  
  // this action is not applicable
  return oldListOfClaims;
};

const accounting = (balance = 100, action) => {
  if (action.type === 'CREATE_CLAIM') {
    return balance - action.payload.amountOfMoneyToCollect;
  } else if (action.type === 'CREATE_POLICY') {
    return balance + action.payload.amount;
  }
  
  return balance;
};

const policies = (listOfPolicies = [], action) => {
  if (action.type === 'CREATE_POLICY') {
    return [...listOfPolicies, action.payload.name];
  } else if (action.type === 'DELETE_POLICY') {
    return listOfPolicies.filter(name => name !== action.payload.name);
  }
  
  return listOfPolicies;
};

// Redux
const { createStore, combineReducers } = Redux;

const ourDepartments = combineReducers({
  accounting: accounting,
  claimsHistory: claimsHistory,
  policies: policies
});

const store = createStore(ourDepartments);

createPolicy('Alex', 20)
createClaim('Alex', 120)
deletePolicy('Alex')

store.dispatch(createPolicy('Alex', 20));
store.dispatch(createPolicy('Bob', 30));
store.dispatch(createPolicy('Cat', 40));

// store.dispatch(createClaim('Alex', 120));
// store.dispatch(createClaim('Cat', 50));

// store.dispatch(deletePolicy('Bob'));

console.log(store.getState());