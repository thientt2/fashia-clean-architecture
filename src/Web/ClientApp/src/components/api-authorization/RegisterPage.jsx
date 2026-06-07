import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuth } from './AuthContext';

const MIN_PASSWORD_LENGTH = 8;

function validateEmail(value) {
  return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value);
}

export function RegisterPage() {
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [firstNameTouched, setFirstNameTouched] = useState(false);
  const [lastNameTouched, setLastNameTouched] = useState(false);
  const [emailTouched, setEmailTouched] = useState(false);
  const [passwordTouched, setPasswordTouched] = useState(false);
  const [error, setError] = useState('');
  const { register } = useAuth();
  const navigate = useNavigate();

  const firstNameValid = firstName.trim().length > 0 && firstName.trim().length <= 100;
  const lastNameValid = lastName.trim().length > 0 && lastName.trim().length <= 100;
  const emailValid = validateEmail(email);
  const passwordValid = password.length >= MIN_PASSWORD_LENGTH;

  const firstNameInvalid = firstNameTouched ? !firstNameValid : undefined;
  const lastNameInvalid = lastNameTouched ? !lastNameValid : undefined;
  const emailInvalid = emailTouched ? !emailValid : undefined;
  const passwordInvalid = passwordTouched ? !passwordValid : undefined;

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setFirstNameTouched(true);
    setLastNameTouched(true);
    setEmailTouched(true);
    setPasswordTouched(true);
    if (!firstNameValid || !lastNameValid || !emailValid || !passwordValid) return;
    try {
      await register(email, password, firstName.trim(), lastName.trim());
      navigate('/login');
    } catch {
      setError('Registration failed. Please try again.');
    }
  };

  return (
    <article>
      <h2>Register</h2>
      {error && <p className="error">{error}</p>}
      <form onSubmit={handleSubmit}>
        <label htmlFor="first-name">First name</label>
        <input type="text" id="first-name" autoComplete="given-name"
          value={firstName}
          onChange={e => setFirstName(e.target.value)}
          onBlur={() => setFirstNameTouched(true)}
          aria-invalid={firstNameInvalid}
          aria-describedby="first-name-helper" />
        <small id="first-name-helper">
          {firstNameTouched && !firstNameValid ? 'Please enter your first name.' : ''}
        </small>
        <label htmlFor="last-name">Last name</label>
        <input type="text" id="last-name" autoComplete="family-name"
          value={lastName}
          onChange={e => setLastName(e.target.value)}
          onBlur={() => setLastNameTouched(true)}
          aria-invalid={lastNameInvalid}
          aria-describedby="last-name-helper" />
        <small id="last-name-helper">
          {lastNameTouched && !lastNameValid ? 'Please enter your last name.' : ''}
        </small>
        <label htmlFor="email">Email</label>
        <input type="email" id="email" autoComplete="username"
          value={email}
          onChange={e => setEmail(e.target.value)}
          onBlur={() => setEmailTouched(true)}
          aria-invalid={emailInvalid}
          aria-describedby="email-helper" />
        <small id="email-helper">
          {emailTouched && !emailValid ? 'Please enter a valid email address.' : ''}
        </small>
        <label htmlFor="password">Password</label>
        <input type="password" id="password" autoComplete="new-password"
          value={password}
          onChange={e => setPassword(e.target.value)}
          onBlur={() => setPasswordTouched(true)}
          aria-invalid={passwordInvalid}
          aria-describedby="password-helper" />
        <small id="password-helper">
          {passwordTouched && !passwordValid
            ? `Password must be at least ${MIN_PASSWORD_LENGTH} characters.`
            : ''}
        </small>
        <button type="submit">Register</button>
        <p style={{ marginTop: '1rem' }}>Already have an account? <Link to="/login">Log in</Link></p>
      </form>
    </article>
  );
}
