import React, { useState, useEffect } from 'react';
import PropertyCard from '../components/PropertyCard';
import { Property } from '../redux/slices/propertySlice';

const Favorites: React.FC = () => {
  const [properties, setProperties] = useState<Property[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    const fetchFavorites = async () => {
      const token = localStorage.getItem('token');
      if (!token) {
        setError('Please login to view favorites');
        setLoading(false);
        return;
      }

      try {
        const response = await fetch('http://localhost:5181/api/favorites', {
          headers: { Authorization: `Bearer ${token}` },
        });

        if (response.ok) {
          const data = await response.json();
          setProperties(data);
        } else {
          setError('Failed to fetch favorites');
        }
      } catch (err) {
        setError('An error occurred');
      }
      setLoading(false);
    };

    fetchFavorites();
  }, []);

  const handleRemoveFavorite = async (propertyId: string) => {
    const token = localStorage.getItem('token');
    if (!token) return;

    try {
      const response = await fetch(`http://localhost:5181/api/favorites/${propertyId}`, {
        method: 'DELETE',
        headers: { Authorization: `Bearer ${token}` },
      });

      if (response.ok) {
        setProperties(properties.filter(p => p.id !== propertyId));
      }
    } catch (err) {
      setError('Failed to remove favorite');
    }
  };

  if (loading) return <div>Loading...</div>;
  if (error) return <div className="text-red-500">{error}</div>;

  return (
    <div className="container mx-auto px-4 py-8">
      <h1 className="text-3xl font-bold mb-8">My Favorites</h1>
      {properties.length === 0 ? (
        <p>No favorite properties yet.</p>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {properties.map((property) => (
            <div key={property.id} className="relative">
              <PropertyCard property={property} />
              <button
                onClick={() => handleRemoveFavorite(property.id)}
                className="absolute top-2 right-2 bg-red-500 text-white px-2 py-1 rounded hover:bg-red-600"
              >
                Remove
              </button>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default Favorites;