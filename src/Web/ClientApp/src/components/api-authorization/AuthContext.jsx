import { createContext, useContext, useState, useEffect } from 'react';
import { AuthClient, UsersClient, LoginRequest, RegisterCustomerCommand } from '../../web-api-client';

const AuthContext = createContext(null);

const client = new UsersClient();
const authClient = new AuthClient();

export function AuthProvider({ children }) {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    client.infoGET()
      .then(() => setIsAuthenticated(true))
      .catch(() => setIsAuthenticated(false))
      .finally(() => setIsLoading(false));
  }, []);

  const login = (email, password) =>
    client.login(true, undefined, new LoginRequest({ email, password }))
      .then(() => setIsAuthenticated(true));

  const register = (email, password, firstName, lastName, phoneNumber) =>
    authClient.registerCustomer(new RegisterCustomerCommand({
      email,
      password,
      firstName,
      lastName,
      phoneNumber
    }));
    
  const logout = () =>
    client.logout({})
      .then(() => setIsAuthenticated(false));

  return (
    <AuthContext.Provider value={{ isAuthenticated, isLoading, login, register, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export const useAuth = () => useContext(AuthContext);
