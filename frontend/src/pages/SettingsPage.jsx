import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import toast from "react-hot-toast";
import api from "../services/api";
import ToggleSwitch from "../components/Shared/ToggleSwitch";
import ConfirmModal from "../components/Shared/ConfirmModal";
import EmailToSupport from "../features/settings/EmailToSupportForm";
import "./SettingsPage.scss";

export default function SettingsPage({
  isPublic,
  isTheOnlyUserMode,
  onToggleTheOnlyUserMode,
}) {
  const [isUserOnlyConfirmOpen, setIsUserOnlyConfirmOpen] = useState(false);
  const [isToSupportFormOpen, setIsToSuppotFormOpen] = useState(false);
  const [isDeleteAccountModalOpen, setIsDeleteAccountModalOpen] =
    useState(null);
  const queryClient = useQueryClient();

  const { register, setValue } = useForm({
    defaultValues: {
      isPublic: isPublic ?? false,
      isTheOnlyUserMode: isTheOnlyUserMode,
    },
  });

  // sync with ToggleSwitch
  useEffect(() => {
    setValue("isPublic", isPublic);
  }, [isPublic, setValue]);

  const switchPublicityMutation = useMutation({
    mutationFn: async () => {
      await api.post("/users/switch-my-publicity");
    },
    onSuccess: () => {
      toast.success("Публічність змінена успішно");
    },
  });

  const makeSetsPrivateMutation = useMutation({
    mutationFn: async () => {
      await api.post("/users/make-my-sets-private");
    },
    onSuccess: () => {
      onToggleTheOnlyUserMode(true);
      queryClient.invalidateQueries(["sets"]);
      toast.success("Режим єдиного користувача активовано");
    },
    onError: () => {
      toast.error("Не вдалося активувати режим єдиного користувача");
    },
  });

  const deleteAccountMutation = useMutation({
    mutationFn: async () => {
      await api.delete("/users/me");
    },
    onSuccess: () => {
      window.location.href = "/";
    },
    onError: () => {
      toast.error("Акаунт видалити не вдалося.");
    },
  });
  return (
    <>
      <div className="settings-page">
        <div className="settings-container">
          <div className="settings-card">
            <h3 className="title">🔓 Приватність та інтерфейс</h3>

            <div className="setting-row">
              <div className="setting-info">
                <label>Приватний профіль</label>
                <p>Заборонити іншим бачити інформацію про вас та ваші успіхи</p>
              </div>
              <ToggleSwitch
                id="is-public"
                registerProps={register("isPublic", {
                  onChange: () => switchPublicityMutation.mutate(),
                })}
              />
            </div>

            <div className="setting-row">
              <div className="setting-info">
                <label>Режим єдиного користувача</label>
                <p>
                  Повністю ізолює вас у системі: інші користувачі приховуються,
                  а всі ваші сети автоматично стають приватними
                </p>
              </div>
              <ToggleSwitch
                id="is-the-only-user"
                registerProps={{
                  checked: isTheOnlyUserMode,
                  onChange: (e) => {
                    if (e.target.checked) {
                      setIsUserOnlyConfirmOpen(true);
                    } else {
                      onToggleTheOnlyUserMode(false);
                    }
                  },
                }}
              />
            </div>
          </div>

          <button
            type="button"
            className="btn-secondary feedback-btn"
            onClick={() => setIsToSuppotFormOpen(true)}
          >
            Написати нам
          </button>

          <div className="settings-card danger-card">
            <h3 className="title">⚠️ Небезпечна зона</h3>
            <div className="setting-row">
              <div className="setting-info">
                <label className="danger-label">Видалити профіль</label>
                <p>Це видалить усі ваші дані назавжди</p>
              </div>
              <button
                type="button"
                className="danger-button"
                onClick={() => setIsDeleteAccountModalOpen(true)}
              >
                Видалити акаунт
              </button>
            </div>
          </div>
        </div>
      </div>
      {isUserOnlyConfirmOpen && (
        <ConfirmModal
          text="Це зробить всі ваші сети приватними і сховає сети інших. Ви впевнені?"
          onConfirm={() => {
            makeSetsPrivateMutation.mutate();
            setIsUserOnlyConfirmOpen(false);
          }}
          onCancel={() => {
            setIsUserOnlyConfirmOpen(false);
          }}
        />
      )}
      {isDeleteAccountModalOpen && (
        <ConfirmModal
          text="Це видалить ваш акаунт назавжди, ви впевнені?"
          onConfirm={() => {
            deleteAccountMutation.mutate();
            setIsDeleteAccountModalOpen(false);
          }}
          onCancel={() => {
            setIsDeleteAccountModalOpen(false);
          }}
        />
      )}
      {isToSupportFormOpen && (
        <EmailToSupport
          isOpen={isToSupportFormOpen}
          onClose={() => setIsToSuppotFormOpen(false)}
        />
      )}
    </>
  );
}
