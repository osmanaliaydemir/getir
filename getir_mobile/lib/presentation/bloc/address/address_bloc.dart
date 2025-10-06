import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:equatable/equatable.dart';
import '../../../domain/entities/address.dart';
import '../../../domain/usecases/address_usecases.dart';
import '../../../data/datasources/address_datasource.dart';

// Events
abstract class AddressEvent extends Equatable {
  const AddressEvent();

  @override
  List<Object?> get props => [];
}

class LoadUserAddresses extends AddressEvent {}

class LoadAddressById extends AddressEvent {
  final String addressId;

  const LoadAddressById(this.addressId);

  @override
  List<Object> get props => [addressId];
}

class CreateAddress extends AddressEvent {
  final CreateAddressRequest request;

  const CreateAddress(this.request);

  @override
  List<Object> get props => [request];
}

class UpdateAddress extends AddressEvent {
  final String addressId;
  final UpdateAddressRequest request;

  const UpdateAddress(this.addressId, this.request);

  @override
  List<Object> get props => [addressId, request];
}

class DeleteAddress extends AddressEvent {
  final String addressId;

  const DeleteAddress(this.addressId);

  @override
  List<Object> get props => [addressId];
}

class SetDefaultAddress extends AddressEvent {
  final String addressId;

  const SetDefaultAddress(this.addressId);

  @override
  List<Object> get props => [addressId];
}

// States
abstract class AddressState extends Equatable {
  const AddressState();

  @override
  List<Object?> get props => [];
}

class AddressInitial extends AddressState {}

class AddressLoading extends AddressState {}

class AddressesLoaded extends AddressState {
  final List<UserAddress> addresses;

  const AddressesLoaded(this.addresses);

  @override
  List<Object> get props => [addresses];
}

class AddressLoaded extends AddressState {
  final UserAddress address;

  const AddressLoaded(this.address);

  @override
  List<Object> get props => [address];
}

class AddressCreated extends AddressState {
  final UserAddress address;

  const AddressCreated(this.address);

  @override
  List<Object> get props => [address];
}

class AddressUpdated extends AddressState {
  final UserAddress address;

  const AddressUpdated(this.address);

  @override
  List<Object> get props => [address];
}

class AddressDeleted extends AddressState {
  final String addressId;

  const AddressDeleted(this.addressId);

  @override
  List<Object> get props => [addressId];
}

class DefaultAddressSet extends AddressState {
  final UserAddress address;

  const DefaultAddressSet(this.address);

  @override
  List<Object> get props => [address];
}

class AddressError extends AddressState {
  final String message;

  const AddressError(this.message);

  @override
  List<Object> get props => [message];
}

// BLoC
class AddressBloc extends Bloc<AddressEvent, AddressState> {
  final GetUserAddressesUseCase _getUserAddressesUseCase;
  final GetAddressByIdUseCase _getAddressByIdUseCase;
  final CreateAddressUseCase _createAddressUseCase;
  final UpdateAddressUseCase _updateAddressUseCase;
  final DeleteAddressUseCase _deleteAddressUseCase;
  final SetDefaultAddressUseCase _setDefaultAddressUseCase;

  AddressBloc({
    required GetUserAddressesUseCase getUserAddressesUseCase,
    required GetAddressByIdUseCase getAddressByIdUseCase,
    required CreateAddressUseCase createAddressUseCase,
    required UpdateAddressUseCase updateAddressUseCase,
    required DeleteAddressUseCase deleteAddressUseCase,
    required SetDefaultAddressUseCase setDefaultAddressUseCase,
  })  : _getUserAddressesUseCase = getUserAddressesUseCase,
        _getAddressByIdUseCase = getAddressByIdUseCase,
        _createAddressUseCase = createAddressUseCase,
        _updateAddressUseCase = updateAddressUseCase,
        _deleteAddressUseCase = deleteAddressUseCase,
        _setDefaultAddressUseCase = setDefaultAddressUseCase,
        super(AddressInitial()) {
    on<LoadUserAddresses>(_onLoadUserAddresses);
    on<LoadAddressById>(_onLoadAddressById);
    on<CreateAddress>(_onCreateAddress);
    on<UpdateAddress>(_onUpdateAddress);
    on<DeleteAddress>(_onDeleteAddress);
    on<SetDefaultAddress>(_onSetDefaultAddress);
  }

  Future<void> _onLoadUserAddresses(
    LoadUserAddresses event,
    Emitter<AddressState> emit,
  ) async {
    emit(AddressLoading());
    try {
      final addresses = await _getUserAddressesUseCase();
      emit(AddressesLoaded(addresses));
    } catch (e) {
      emit(AddressError(e.toString()));
    }
  }

  Future<void> _onLoadAddressById(
    LoadAddressById event,
    Emitter<AddressState> emit,
  ) async {
    emit(AddressLoading());
    try {
      final address = await _getAddressByIdUseCase(event.addressId);
      emit(AddressLoaded(address));
    } catch (e) {
      emit(AddressError(e.toString()));
    }
  }

  Future<void> _onCreateAddress(
    CreateAddress event,
    Emitter<AddressState> emit,
  ) async {
    emit(AddressLoading());
    try {
      final address = await _createAddressUseCase(event.request);
      emit(AddressCreated(address));
    } catch (e) {
      emit(AddressError(e.toString()));
    }
  }

  Future<void> _onUpdateAddress(
    UpdateAddress event,
    Emitter<AddressState> emit,
  ) async {
    emit(AddressLoading());
    try {
      final address = await _updateAddressUseCase(event.addressId, event.request);
      emit(AddressUpdated(address));
    } catch (e) {
      emit(AddressError(e.toString()));
    }
  }

  Future<void> _onDeleteAddress(
    DeleteAddress event,
    Emitter<AddressState> emit,
  ) async {
    emit(AddressLoading());
    try {
      await _deleteAddressUseCase(event.addressId);
      emit(AddressDeleted(event.addressId));
    } catch (e) {
      emit(AddressError(e.toString()));
    }
  }

  Future<void> _onSetDefaultAddress(
    SetDefaultAddress event,
    Emitter<AddressState> emit,
  ) async {
    emit(AddressLoading());
    try {
      final address = await _setDefaultAddressUseCase(event.addressId);
      emit(DefaultAddressSet(address));
    } catch (e) {
      emit(AddressError(e.toString()));
    }
  }
}
