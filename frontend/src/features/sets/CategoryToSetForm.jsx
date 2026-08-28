import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useCategories } from "../../hooks/useCategories";
import { useState } from "react";
import { ModalWrapper } from "../../components/Shared/ModalWrapper";
import toast from "react-hot-toast";
import api from "../../services/api";
import "./CategoryToSetForm.scss";

export default function CategoryToSetForm({ isOpen, onClose, setId }) {
  const categories = useCategories();
  const queryClient = useQueryClient();

  const [selectedCategoryId, setSelectedCategoryId] = useState("");

  const saveMutation = useMutation({
    mutationFn: (categoryId) =>
      api.post(`my-sets/${setId}/add-category`, null, {
        params: { categoryId },
      }),
    onSuccess: () => {
      queryClient.invalidateQueries(["setInfo", setId]);
      toast.success("Категорію успішно додано");
      onClose();
    },
  });

  const handleSubmit = (e) => {
    e.preventDefault();
    if (!selectedCategoryId) return;
    saveMutation.mutate(selectedCategoryId);
  };

  return (
    <ModalWrapper
      isOpen={isOpen}
      onClose={onClose}
      showCloseButton={true}
      isCentered={true}
      size="sm"
    >
      <form className="add-category-to-set-form" onSubmit={handleSubmit}>
        <select
          name="category"
          className="sort-select"
          value={selectedCategoryId}
          onChange={(e) => setSelectedCategoryId(e.target.value)}
        >
          <option value="" disabled>
            Оберіть категорію
          </option>
          {categories?.map((cat) => (
            <option key={cat.id} value={cat.id}>
              {cat.name}
            </option>
          ))}
        </select>
        <button
          type="submit"
          className="primary-button"
          disabled={!selectedCategoryId || saveMutation.isPending}
        >
          {saveMutation.isPending ? "Збереження..." : "Зберегти"}
        </button>
      </form>
    </ModalWrapper>
  );
}
